<script lang="ts">
    import type { FileSummaryDTO } from '$lib/api_client';
    import FileDisplay from '$lib/layout/FileDisplay.svelte';
    import DatasetFileTreeNode from '$lib/components/datasets/DatasetFileTreeNode.svelte';
    import { buildFileTree } from '$lib/components/datasets/file-tree';

    type FilesViewMode = 'list' | 'tree';

    let { files = [] } = $props<{
        files?: FileSummaryDTO[] | null;
    }>();

    let viewMode = $state<FilesViewMode>('list');

    const safeFiles = $derived(files ?? []);
    const tree = $derived(buildFileTree(safeFiles));

    const setViewMode = (nextMode: FilesViewMode) => {
        viewMode = nextMode;
    };
</script>

<section class="mt-8 space-y-3">
    <div class="flex flex-wrap items-center justify-between gap-3">
        <h2 class="text-lg font-semibold text-gray-800">Files</h2>
        <div class="inline-flex overflow-hidden rounded-md border border-gray-300 bg-white">
            <button
                type="button"
                class="cursor-pointer px-3 py-1.5 text-sm"
                class:bg-blue-600={viewMode === 'list'}
                class:text-white={viewMode === 'list'}
                class:bg-white={viewMode !== 'list'}
                class:text-gray-700={viewMode !== 'list'}
                onclick={() => setViewMode('list')}
            >
                List
            </button>
            <button
                type="button"
                class="cursor-pointer border-l border-gray-300 px-3 py-1.5 text-sm"
                class:bg-blue-600={viewMode === 'tree'}
                class:text-white={viewMode === 'tree'}
                class:bg-white={viewMode !== 'tree'}
                class:text-gray-700={viewMode !== 'tree'}
                onclick={() => setViewMode('tree')}
            >
                Tree
            </button>
        </div>
    </div>

    {#if safeFiles.length === 0}
        <p class="rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-600">No files</p>
    {:else if viewMode === 'list'}
        <div class="space-y-3">
            {#each safeFiles as file (file.id)}
                <FileDisplay
                    title={file.name ?? file.id ?? 'unknown'}
                    fileSlug={`file-list-${file.id ?? file.name ?? 'file'}`}
                    file={file}
                    includeHiddenMode={true}
                    defaultDisplayMode="hidden"
                />
            {/each}
        </div>
    {:else}
        <div class="space-y-2 rounded-xl border border-gray-200 bg-gray-50 p-3">
            {#each tree as node (node.path)}
                <DatasetFileTreeNode node={node} />
            {/each}
        </div>
    {/if}
</section>
