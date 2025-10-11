<script lang="ts">
    import type {FileSummaryDTO} from "$lib/api_client";
	import ImagePlugin from "./displayPlugins/ImagePlugin.svelte";
    import {FileSize} from "$lib/data/FileSize";

    export let title: string;
    export let fileSlug: string;
    export let file: FileSummaryDTO;

    let size = file?.size? new FileSize(file.size) : undefined;

    let fileType: string | undefined = undefined;
    if (file?.contentType?.abbreviation ?? '' in ["png", "jpg", "jpeg"]) fileType = 'image';
    else if (file?.contentType?.abbreviation ?? '' in ["mp4", "avi", "mov"]) fileType = 'video';
    else if (file?.contentType?.abbreviation ?? '' in ["pdf"]) fileType = 'pdf';
</script>

<div id="{fileSlug}">

    <section class="overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm">
        <header class="flex items-center justify-between border-b border-gray-200 bg-gray-50 px-4 py-2">
            <h2 class="font-medium text-gray-800">{title}</h2>
            <span class="text-xs text-gray-500">{size? size.getString('*B', true, 1) : 'undefined'}</span>
        </header>
        <div class="p-4">
            {#if fileType === 'image'}
                <ImagePlugin uri={file.downloadURI ?? ''} />
            <!--{:else if fileType === 'video'}-->
            <!--    <video controls>-->
            <!--        <source src={file.downloadURI ?? ''} type="video/mp4" />-->
            <!--        Your browser does not support the video tag.-->
            <!--    </video>-->
            {:else}
                <p>File type not supported</p>
            {/if}
            <!-- ... -->
        </div>
    </section>

</div>