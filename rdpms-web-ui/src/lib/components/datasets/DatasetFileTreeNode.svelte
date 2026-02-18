<script lang="ts">
    import FileDisplay from '$lib/layout/FileDisplay.svelte';
    import type { FileTreeNode } from '$lib/components/datasets/file-tree';
    import DatasetFileTreeNode from '$lib/components/datasets/DatasetFileTreeNode.svelte';

    let { node, level = 0 } = $props<{
        node: FileTreeNode;
        level?: number;
    }>();

    const toSlug = (value: string): string => value.replace(/[^a-zA-Z0-9_-]/g, '-');
</script>

{#if node.type === 'folder'}
    <details class="rounded-lg border border-gray-200 bg-white" open={level < 2}>
        <summary class="cursor-pointer px-3 py-2 text-sm font-medium text-gray-700">
            {node.name}
        </summary>
        <div class="space-y-2 border-t border-gray-100 p-2 pl-4">
            {#each node.children as child (child.path)}
                <DatasetFileTreeNode node={child} level={level + 1} />
            {/each}
        </div>
    </details>
{:else}
    <FileDisplay
        title={node.name}
        fileSlug={`file-tree-${toSlug(node.path)}`}
        file={node.file}
        includeHiddenMode={true}
        defaultDisplayMode="hidden"
    />
{/if}
