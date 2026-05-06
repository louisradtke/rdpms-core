<script lang="ts">
	import type { DataSetSummaryDTO } from '$lib/api_client';
	import { DataSetsRepository } from '$lib/data/DataSetsRepository';
	import CodeCopyField from '$lib/layout/CodeCopyField.svelte';
	import { getOrFetchConfig, toApiConfig } from '$lib/util/config-helper';

	let { datasets, projectSlug, collectionSlug, onDelete } = $props<{
		datasets: DataSetSummaryDTO[];
		projectSlug: string;
		collectionSlug: string;
		onDelete: () => void;
	}>();

	type GroupedDatasets = {
		key: string;
		label: string;
		sortTime: number;
		datasets: DataSetSummaryDTO[];
	};

	const dayFormatter = new Intl.DateTimeFormat(undefined, {
		weekday: 'short',
		year: 'numeric',
		month: 'short',
		day: 'numeric',
		timeZone: 'UTC'
	});

	const timeFormatter = new Intl.DateTimeFormat(undefined, {
		hour: '2-digit',
		minute: '2-digit',
		second: '2-digit',
		timeZone: 'UTC'
	});

	const asDate = (value?: Date | null) => {
		if (!value) return null;
		return value instanceof Date ? value : new Date(value);
	};

	const datasetTimestamp = (dataset: DataSetSummaryDTO) => {
		return (
			asDate(dataset.beginStampUTC ?? dataset.createdStampUTC ?? dataset.endStampUTC)?.getTime() ??
			Number.NEGATIVE_INFINITY
		);
	};

	const datasetBeginDate = (dataset: DataSetSummaryDTO) => {
		return asDate(dataset.beginStampUTC);
	};

	const dayKey = (value: Date | null) => {
		if (!value) return 'undated';

		const year = value.getUTCFullYear();
		const month = String(value.getUTCMonth() + 1).padStart(2, '0');
		const day = String(value.getUTCDate()).padStart(2, '0');
		return `${year}-${month}-${day}`;
	};

	const dayLabel = (value: Date | null) =>
		value ? `${dayFormatter.format(value)} (UTC)` : 'No begin date';

	const formatTime = (value?: Date | null) => {
		const date = asDate(value);
		return date ? timeFormatter.format(date) : '—';
	};

	const formatFileCount = (dataset: DataSetSummaryDTO) =>
		dataset.fileCount ?? dataset.files?.length ?? 0;

	const totalSize = (dataset: DataSetSummaryDTO) => {
		if (typeof dataset.totalSizeBytes === 'number') return dataset.totalSizeBytes;
		if (!dataset.files) return null;

		return dataset.files.reduce((sum, file) => sum + (file.size ?? 0), 0);
	};

	const formatBytes = (value: number | null) => {
		if (value === null || !Number.isFinite(value)) return '—';
		if (value === 0) return '0 B';

		const units = ['B', 'KB', 'MB', 'GB', 'TB'];
		const exponent = Math.min(Math.floor(Math.log(value) / Math.log(1024)), units.length - 1);
		const scaled = value / Math.pow(1024, exponent);
		const precision = scaled >= 10 || exponent === 0 ? 0 : 1;

		return `${scaled.toFixed(precision)} ${units[exponent]}`;
	};

	const datasetName = (dataset: DataSetSummaryDTO) =>
		dataset.name ?? dataset.slug ?? dataset.id ?? 'Untitled dataset';

	const datasetRouteValue = (dataset: DataSetSummaryDTO) => dataset.slug ?? dataset.id ?? '';

	const groupedDatasets = $derived.by(() => {
		const groups: GroupedDatasets[] = [];

		for (const dataset of datasets) {
			const beginDate = datasetBeginDate(dataset);
			const beginTimestamp = beginDate?.getTime() ?? Number.NEGATIVE_INFINITY;
			const key = dayKey(beginDate);
			const existing = groups.find((group) => group.key === key);

			if (existing) {
				existing.datasets.push(dataset);
				existing.sortTime = Math.max(existing.sortTime, beginTimestamp);
				continue;
			}

			groups.push({
				key,
				label: dayLabel(beginDate),
				sortTime: beginTimestamp,
				datasets: [dataset]
			});
		}

		return groups
			.map((group) => ({
				...group,
				datasets: [...group.datasets].sort((a, b) => datasetTimestamp(b) - datasetTimestamp(a))
			}))
			.sort((a, b) => b.sortTime - a.sortTime);
	});

	let deletingId = $state<string | null>(null);
	let deleteError = $state('');

	async function deleteDataset(dataset: DataSetSummaryDTO): Promise<void> {
		if (!dataset.id) return;
		const confirmed = window.confirm(`Delete dataset "${dataset.name ?? dataset.id}"?`);
		if (!confirmed) return;

		deletingId = dataset.id;
		deleteError = '';

		try {
			const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));
			await repo.deleteById(dataset.id);
			onDelete();
		} catch (e) {
			deleteError = e instanceof Error ? e.message : 'Failed to delete dataset.';
		} finally {
			deletingId = null;
		}
	}
</script>

{#if deleteError}
	<p class="mb-2 rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
		{deleteError}
	</p>
{/if}

{#if datasets.length === 0}
	<p class="rounded-md border border-gray-200 bg-white px-4 py-5 text-sm text-gray-600">
		No datasets in this collection.
	</p>
{:else}
	<div class="overflow-x-auto rounded-md border border-gray-200 bg-white">
		<table class="w-full min-w-[760px] table-fixed text-sm">
			<colgroup>
				<col class="w-[46%]" />
				<col class="w-[12%]" />
				<col class="w-[12%]" />
				<col class="w-[9%]" />
				<col class="w-[13%]" />
				<col class="w-[8%]" />
			</colgroup>
			<thead class="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
				<tr>
					<th class="px-4 py-2 text-left font-medium">Dataset</th>
					<th class="px-3 py-2 text-left font-medium">Begin</th>
					<th class="px-3 py-2 text-left font-medium">End</th>
					<th class="px-3 py-2 text-right font-medium">Count</th>
					<th class="px-3 py-2 text-right font-medium">Sum</th>
					<th class="px-4 py-2 text-right font-medium">Actions</th>
				</tr>
			</thead>
			<tbody class="divide-y divide-gray-100">
				{#each groupedDatasets as group (group.key)}
					<tr class="bg-gray-100/80">
						<th
							colspan="6"
							class="px-4 py-2 text-left text-xs font-semibold uppercase tracking-wide text-gray-600"
						>
							{group.label}
						</th>
					</tr>
					{#each group.datasets as dataset, index (dataset.id ?? dataset.slug ?? `${group.key}-${index}`)}
						{@const routeValue = datasetRouteValue(dataset)}
						<tr class="hover:bg-gray-50">
							<td class="px-4 py-3 align-top">
								<div class="min-w-0">
									{#if routeValue}
										<a
											href="/projects/{projectSlug}/c/{collectionSlug}/{routeValue}"
											class="font-medium text-blue-600 hover:underline break-words [overflow-wrap:anywhere]"
											title={datasetName(dataset)}
										>
											{datasetName(dataset)}
										</a>
									{:else}
										<span class="font-medium text-gray-900 break-words [overflow-wrap:anywhere]">
											{datasetName(dataset)}
										</span>
									{/if}

									{#if dataset.id}
										<div class="mt-1 flex min-w-0 items-center gap-1 text-xs text-gray-500">
											<span class="shrink-0">ID</span>
											<span class="min-w-0 truncate font-mono" title={dataset.id}>{dataset.id}</span
											>
											<CodeCopyField text={dataset.id} copyOnly={true} tooltip="Copy dataset ID" />
										</div>
									{/if}
								</div>
							</td>
							<td class="whitespace-nowrap px-3 py-3 align-top font-mono text-xs text-gray-700">
								{formatTime(dataset.beginStampUTC)}
							</td>
							<td class="whitespace-nowrap px-3 py-3 align-top font-mono text-xs text-gray-700">
								{formatTime(dataset.endStampUTC)}
							</td>
							<td
								class="whitespace-nowrap px-3 py-3 text-right align-top tabular-nums text-gray-700"
							>
								{formatFileCount(dataset)}
							</td>
							<td
								class="whitespace-nowrap px-3 py-3 text-right align-top tabular-nums text-gray-700"
							>
								{formatBytes(totalSize(dataset))}
							</td>
							<td class="px-4 py-3 text-right align-top">
								<button
									type="button"
									class="inline-flex size-8 items-center justify-center rounded-md text-red-600 hover:bg-red-50 hover:text-red-700 disabled:cursor-default disabled:opacity-50"
									aria-label={`Delete dataset ${datasetName(dataset)}`}
									title={deletingId === dataset.id ? 'Deleting dataset' : 'Delete dataset'}
									disabled={deletingId === dataset.id}
									onclick={() => deleteDataset(dataset)}
								>
									{#if deletingId === dataset.id}
										<span aria-hidden="true" class="text-xs font-semibold">...</span>
									{:else}
										<svg aria-hidden="true" viewBox="0 0 20 20" fill="currentColor" class="size-4">
											<path
												fill-rule="evenodd"
												d="M8.5 2a1.5 1.5 0 0 0-1.415 1H4.75a.75.75 0 0 0 0 1.5h10.5a.75.75 0 0 0 0-1.5h-2.335A1.5 1.5 0 0 0 11.5 2h-3Zm-2.8 4a.75.75 0 0 0-.747.812l.7 8.4A2 2 0 0 0 7.647 17h4.706a2 2 0 0 0 1.993-1.788l.7-8.4A.75.75 0 0 0 14.3 6H5.7Zm2.55 2.25a.75.75 0 0 1 .75.75v5a.75.75 0 0 1-1.5 0V9a.75.75 0 0 1 .75-.75Zm3.5 0a.75.75 0 0 1 .75.75v5a.75.75 0 0 1-1.5 0V9a.75.75 0 0 1 .75-.75Z"
												clip-rule="evenodd"
											/>
										</svg>
									{/if}
								</button>
							</td>
						</tr>
					{/each}
				{/each}
			</tbody>
		</table>
	</div>
{/if}
