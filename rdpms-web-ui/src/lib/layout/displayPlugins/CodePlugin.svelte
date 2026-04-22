<script lang="ts">
	import LoadingCircle from '$lib/layout/LoadingCircle.svelte';
	import type { CorePluginPolicy } from '$lib/layout/displayPlugins/plugin-registry';

	let {
		dataUri,
		policy
	}: {
		dataUri: string;
		policy?: CorePluginPolicy;
	} = $props();

	const DEFAULT_MAX_SIZE = 2 * 1024 * 1024;
	let maxSize = $derived(policy?.display?.maxBytes ?? DEFAULT_MAX_SIZE);

	async function fetchTextContent(uri: string): Promise<{ text: string; truncated: boolean }> {
		if (!uri) {
			throw new Error('No URI available');
		}

		const response = await fetch(uri);
		if (!response.ok) {
			throw new Error(`Failed to fetch file: ${response.status} ${response.statusText}`);
		}

		const body = response.body;
		if (!body) {
			const text = await response.text();
			const actualSize = new Blob([text]).size;
			return {
				text: actualSize > maxSize ? text.slice(0, maxSize) : text,
				truncated: actualSize > maxSize
			};
		}

		const reader = body.getReader();
		const chunks: Uint8Array[] = [];
		let receivedBytes = 0;
		let truncated = false;

		try {
			while (receivedBytes < maxSize) {
				const { done, value } = await reader.read();
				if (done) {
					break;
				}

				if (!value) {
					continue;
				}

				const remainingBytes = maxSize - receivedBytes;
				if (value.byteLength <= remainingBytes) {
					chunks.push(value);
					receivedBytes += value.byteLength;
					continue;
				}

				chunks.push(value.subarray(0, remainingBytes));
				receivedBytes += remainingBytes;
				truncated = true;
				break;
			}
		} finally {
			await reader.cancel().catch(() => undefined);
		}

		const merged = new Uint8Array(receivedBytes);
		let offset = 0;
		for (const chunk of chunks) {
			merged.set(chunk, offset);
			offset += chunk.byteLength;
		}

		return {
			text: new TextDecoder().decode(merged),
			truncated
		};
	}

	const contentPromise = $derived(fetchTextContent(dataUri));
</script>

{#await contentPromise}
	<div class="flex justify-center items-center">
		<LoadingCircle />
	</div>
{:then content}
	<div class="space-y-2">
		{#if content.truncated}
			<div class="rounded border border-amber-200 bg-amber-50 p-2 text-sm text-amber-900">
				Showing the first {(maxSize / 1024 / 1024).toFixed(0)} MiB only.
			</div>
		{/if}
		<pre
			class="max-h-[70vh] overflow-auto rounded border border-gray-200 bg-gray-50 p-3 text-xs text-gray-800">{content.text}</pre>
	</div>
{:catch error}
	<div class="rounded border border-red-200 bg-red-50 p-2 text-sm text-red-800">
		{error?.message ?? 'Failed to load text preview'}
	</div>
{/await}
