<script lang="ts">
    let { uri }: { uri: string } = $props();
</script>

{#if uri}
    <iframe
        src={uri}
        title="PDF preview"
        class="h-[70vh] w-full rounded border border-gray-200"
    ></iframe>
{:else}
    <p class="text-center text-sm text-gray-500">PDF preview unavailable</p>
{/if}
