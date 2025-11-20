<script lang="ts">
    import type {FileSummaryDTO} from "$lib/api_client";
	import ImagePlugin from "./displayPlugins/ImagePlugin.svelte";
    import {FileSize} from "$lib/data/FileSize";
    import CodeCopyField from "$lib/layout/CodeCopyField.svelte";
    import TablePlugin from "$lib/layout/displayPlugins/TablePlugin.svelte";

    function downloadName(file: FileSummaryDTO): string {
        if (file.name) return file.name;
        if (file.contentType?.abbreviation) return file.id + '.' + file.contentType.abbreviation;
        return "unknown";
    }

    async function handleDownload(event: Event, file: FileSummaryDTO) {
        event.preventDefault();
        
        if (!file.downloadURI) return;
        
        try {
            const response = await fetch(file.downloadURI);
            if (!response.ok) throw new Error('Download failed');
            
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = downloadName(file);
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error('Download failed:', error);
            // Fallback to opening in new tab
            window.open(file.downloadURI, '_blank');
        }
    }

    export let title: string;
    export let fileSlug: string;
    export let file: FileSummaryDTO;

    let size = !Number.isNaN(file?.size) ? new FileSize(file.size || 0) : undefined;

    let fileType: string | undefined = undefined;
    if (["png", "jpg", "jpeg"].includes(file?.contentType?.abbreviation ?? '')) fileType = 'image';
    else if (["mp4", "avi", "mov"].includes(file?.contentType?.abbreviation ?? '')) fileType = 'video';
    else if (["pdf"].includes(file?.contentType?.abbreviation ?? '')) fileType = 'pdf';
    else if (["csv"].includes(file?.contentType?.abbreviation ?? '')) fileType = 'csv';
</script>

<div id="{fileSlug}">

    <section class="overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm">
        <header class="flex items-center justify-between border-b border-gray-200 bg-gray-50 px-4 py-2">
            <h2 class="font-medium text-gray-800">{title}</h2>
            <div class="flex items-center gap-4">
                <span class="text-xs text-gray-500">{size !== undefined ? size.getString('*B', true, 1) : 'undefined'}</span>
                <button on:click={(e) => handleDownload(e, file)}
                   class="text-gray-500 hover:text-gray-700 cursor-pointer" aria-label="Download">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24"
                         stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                              d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
                    </svg>
                </button>
                <CodeCopyField text={file.downloadURI ?? ''} copyOnly={true} tooltip="Copy download URI" />
            </div>
        </header>
        <div class="p-4">
            {#if fileType === 'image'}
                <ImagePlugin uri={file.downloadURI ?? ''} />
            {:else if fileType === 'csv'}
                <TablePlugin dataUri="{file.downloadURI ?? ''}" />
            <!--{:else if fileType === 'video'}-->
            <!--    <video controls>-->
            <!--        <source src={file.downloadURI ?? ''} type="video/mp4" />-->
            <!--        Your browser does not support the video tag.-->
            <!--    </video>-->
            {:else}
                <p class="text-center text-sm text-gray-500">File type not supported</p>
            {/if}
            <!-- ... -->
        </div>
    </section>

</div>