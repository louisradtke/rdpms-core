<script lang="ts">
    import LoadingCircle from '$lib/layout/LoadingCircle.svelte';

    let { dataUri }: { dataUri: string } = $props();

    const MAX_SIZE = 2 * 1024 * 1024;

    async function fetchTextContent(uri: string): Promise<string> {
        if (!uri) {
            throw new Error('No URI available');
        }

        const response = await fetch(uri);
        if (!response.ok) {
            throw new Error(`Failed to fetch file: ${response.status} ${response.statusText}`);
        }

        const contentLength = response.headers.get('content-length');
        if (contentLength && Number.parseInt(contentLength, 10) > MAX_SIZE) {
            throw new Error('File too large for inline preview (max 2 MB)');
        }

        const text = await response.text();
        const actualSize = new Blob([text]).size;
        if (actualSize > MAX_SIZE) {
            throw new Error('File too large for inline preview (max 2 MB)');
        }

        return text;
    }

    const contentPromise = $derived(fetchTextContent(dataUri));
</script>

{#await contentPromise}
    <div class="flex justify-center items-center">
        <LoadingCircle />
    </div>
{:then content}
    <pre class="max-h-[70vh] overflow-auto rounded border border-gray-200 bg-gray-50 p-3 text-xs text-gray-800">{content}</pre>
{:catch error}
    <div class="rounded border border-red-200 bg-red-50 p-2 text-sm text-red-800">{error?.message ?? 'Failed to load text preview'}</div>
{/await}
