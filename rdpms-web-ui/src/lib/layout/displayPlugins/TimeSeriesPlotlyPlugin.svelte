<script lang="ts">
	import LoadingCircle from '$lib/layout/LoadingCircle.svelte';
	import type { CorePluginPolicy } from '$lib/layout/displayPlugins/plugin-registry';
	import type { Config, Data, Layout, PlotlyHTMLElement } from 'plotly.js';
	import type { Action } from 'svelte/action';

	interface SeriesOptionObject {
		field: string;
		label?: string;
		mode?: 'lines' | 'markers' | 'lines+markers';
	}

	interface TimeSeriesRendererOptions {
		timeField?: string;
		series?: string[] | SeriesOptionObject[];
		mode?: 'lines' | 'markers' | 'lines+markers';
		title?: string;
		xAxisTitle?: string;
		yAxisTitle?: string;
	}

	interface ParsedPlotData {
		data: Partial<Data>[];
		layout: Partial<Layout>;
		config: Partial<Config>;
		seriesCount: number;
		pointCount: number;
		timeField: string;
	}

	let {
		dataUri,
		policy,
		options
	}: {
		dataUri: string;
		policy?: CorePluginPolicy;
		options?: Record<string, unknown>;
	} = $props();

	const DEFAULT_MAX_SIZE = 25 * 1024 * 1024;

	let renderedPlot = $state(false);
	let plotError = $state<string | null>(null);

	const parsedOptions = $derived((options ?? {}) as TimeSeriesRendererOptions);
	const maxSize = $derived(policy?.display?.maxBytes ?? DEFAULT_MAX_SIZE);
	const oversizeMessage = $derived(
		policy?.display?.oversizeMessage ?? 'File too large for Plotly time-series preview (max 25 MiB)'
	);

	function splitCsvLine(line: string): string[] {
		const result: string[] = [];
		let current = '';
		let inQuotes = false;

		for (let i = 0; i < line.length; i += 1) {
			const ch = line[i];
			if (ch === '"') {
				if (inQuotes && line[i + 1] === '"') {
					current += '"';
					i += 1;
				} else {
					inQuotes = !inQuotes;
				}
				continue;
			}

			if (ch === ',' && !inQuotes) {
				result.push(current.trim());
				current = '';
				continue;
			}

			current += ch;
		}

		result.push(current.trim());
		return result;
	}

	function normalizeHeader(value: string): string {
		return value.trim().toLowerCase();
	}

	function resolveSeriesOptions(
		headers: string[],
		rows: Record<string, string>[],
		timeField: string,
		options: TimeSeriesRendererOptions
	): SeriesOptionObject[] {
		const configuredSeries = options.series;
		if (Array.isArray(configuredSeries) && configuredSeries.length > 0) {
			return configuredSeries.map((series) =>
				typeof series === 'string' ? { field: series } : series
			);
		}

		const candidates = headers.filter(
			(header) => normalizeHeader(header) !== normalizeHeader(timeField)
		);
		return candidates
			.filter((header) =>
				rows.some((row) => {
					const value = row[header] ?? '';
					return value.length > 0 && Number.isFinite(Number.parseFloat(value));
				})
			)
			.map((field) => ({ field }));
	}

	async function fetchAndParseTimeSeries(): Promise<ParsedPlotData> {
		if (!dataUri) {
			throw new Error('No URI available');
		}

		const response = await fetch(dataUri);
		if (!response.ok) {
			throw new Error(`Failed to fetch file: ${response.status} ${response.statusText}`);
		}

		const contentLength = response.headers.get('content-length');
		if (contentLength && Number.parseInt(contentLength, 10) > maxSize) {
			throw new Error(oversizeMessage);
		}

		const text = await response.text();
		const actualSize = new Blob([text]).size;
		if (actualSize > maxSize) {
			throw new Error(oversizeMessage);
		}

		const lines = text
			.split(/\r?\n/)
			.map((line) => line.trim())
			.filter((line) => line.length > 0);

		if (lines.length < 2) {
			throw new Error('CSV must contain a header and at least one row');
		}

		const headers = splitCsvLine(lines[0]).map((header) => header.replace(/^"|"$/g, ''));
		const timeField = parsedOptions.timeField ?? 'stamp';
		const timeFieldHeader = headers.find(
			(header) => normalizeHeader(header) === normalizeHeader(timeField)
		);

		if (!timeFieldHeader) {
			throw new Error(`Time field "${timeField}" not found in CSV header`);
		}

		const rows = lines.slice(1).map((line) => {
			const values = splitCsvLine(line).map((value) => value.replace(/^"|"$/g, ''));
			const row: Record<string, string> = {};
			headers.forEach((header, index) => {
				row[header] = values[index] ?? '';
			});
			return row;
		});

		const seriesOptions = resolveSeriesOptions(headers, rows, timeFieldHeader, parsedOptions);
		if (seriesOptions.length === 0) {
			throw new Error('No numeric series columns available for plotting');
		}

		const xValues = rows.map((row) => row[timeFieldHeader] ?? '');
		const data: Partial<Data>[] = seriesOptions.map((series) => {
			const seriesHeader = headers.find(
				(header) => normalizeHeader(header) === normalizeHeader(series.field)
			);
			if (!seriesHeader) {
				throw new Error(`Series field "${series.field}" not found in CSV header`);
			}

			return {
				type: 'scatter',
				mode: series.mode ?? parsedOptions.mode ?? 'lines',
				name: series.label ?? seriesHeader,
				x: xValues,
				y: rows.map((row) => {
					const value = row[seriesHeader] ?? '';
					const numeric = Number.parseFloat(value);
					return Number.isFinite(numeric) ? numeric : null;
				}),
				connectgaps: false
			};
		});

		return {
			data,
			layout: {
				title: parsedOptions.title ? { text: parsedOptions.title } : undefined,
				margin: { t: parsedOptions.title ? 48 : 20, r: 24, b: 56, l: 64 },
				xaxis: {
					title: { text: parsedOptions.xAxisTitle ?? timeFieldHeader },
					type: 'date'
				},
				yaxis: {
					title: parsedOptions.yAxisTitle ? { text: parsedOptions.yAxisTitle } : undefined
				},
				legend: {
					orientation: 'h',
					y: 1.12
				}
			},
			config: {
				responsive: true,
				displaylogo: false
			},
			seriesCount: data.length,
			pointCount: rows.length,
			timeField: timeFieldHeader
		};
	}

	const plotDataPromise = $derived(fetchAndParseTimeSeries());

	type PlotlyActionParams = {
		plotData: ParsedPlotData;
	};

	const plotlyChart: Action<HTMLDivElement, PlotlyActionParams> = (node, params) => {
		let cancelled = false;
		let currentRun = 0;

		const loadPlotly = async () => {
			const PlotlyModule = await import('plotly.js-dist-min');
			return PlotlyModule.default as {
				newPlot: (
					root: HTMLElement,
					data: Partial<Data>[],
					layout?: Partial<Layout>,
					config?: Partial<Config>
				) => Promise<PlotlyHTMLElement>;
				purge: (root: HTMLElement) => void;
			};
		};

		const loadAndRender = async (nextParams: PlotlyActionParams) => {
			const runId = ++currentRun;
			renderedPlot = false;
			plotError = null;

			const Plotly = await loadPlotly();
			if (cancelled) {
				return;
			}

			try {
				Plotly.purge(node);
				await Plotly.newPlot(
					node,
					nextParams.plotData.data,
					nextParams.plotData.layout,
					nextParams.plotData.config
				);
				if (!cancelled && runId === currentRun) {
					renderedPlot = true;
				}
			} catch (error) {
				if (!cancelled && runId === currentRun) {
					plotError = error instanceof Error ? error.message : 'Failed to render Plotly graph';
					renderedPlot = false;
				}
			}
		};

		loadAndRender(params).catch((error) => {
			if (!cancelled) {
				console.error('Failed to render Plotly chart', error);
				plotError = error instanceof Error ? error.message : 'Failed to render Plotly graph';
				renderedPlot = false;
			}
		});

		return {
			update(nextParams) {
				loadAndRender(nextParams).catch((error) => {
					if (!cancelled) {
						console.error('Failed to update Plotly chart', error);
						plotError = error instanceof Error ? error.message : 'Failed to render Plotly graph';
						renderedPlot = false;
					}
				});
			},
			destroy() {
				cancelled = true;
				import('plotly.js-dist-min')
					.then((PlotlyModule) => {
						const Plotly = PlotlyModule.default as {
							purge: (root: HTMLElement) => void;
						};
						Plotly.purge(node);
					})
					.catch(() => undefined);
				renderedPlot = false;
			}
		};
	};
</script>

{#await plotDataPromise}
	<div class="flex justify-center items-center">
		<LoadingCircle />
	</div>
{:then plotData}
	<div class="space-y-3">
		<div class="grid grid-cols-1 gap-2 text-xs text-gray-600 sm:grid-cols-3">
			<div><span class="font-medium text-gray-800">Time field:</span> {plotData.timeField}</div>
			<div><span class="font-medium text-gray-800">Series:</span> {plotData.seriesCount}</div>
			<div><span class="font-medium text-gray-800">Rows:</span> {plotData.pointCount}</div>
		</div>
		<div class="rounded border border-gray-200 bg-white p-2">
			<div use:plotlyChart={{ plotData }} class="h-[28rem] w-full"></div>
			{#if plotError}
				<p class="px-2 pb-1 text-xs text-red-700">{plotError}</p>
			{:else if !renderedPlot}
				<p class="px-2 pb-1 text-xs text-gray-500">Initializing Plotly graph...</p>
			{/if}
		</div>
	</div>
{:catch error}
	<div class="rounded border border-red-200 bg-red-50 p-2 text-sm text-red-800">
		{error?.message ?? 'Failed to load time-series preview'}
	</div>
{/await}
