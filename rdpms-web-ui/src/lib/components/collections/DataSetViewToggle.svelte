<script lang="ts">
    type CollectionView = 'basic' | 'dataset-metadata' | 'file-metadata';
    let { activeView, onChange } = $props<{
        activeView: CollectionView;
        onChange: (view: CollectionView) => void;
    }>();
</script>

<div class="mb-3 flex flex-wrap items-center gap-2">
    <span class="text-sm text-gray-600">View:</span>
    <button
            class="rounded border px-3 py-1 text-sm"
            class:bg-blue-600={activeView === 'basic'}
            class:text-white={activeView === 'basic'}
            class:border-blue-600={activeView === 'basic'}
            class:bg-white={activeView !== 'basic'}
            class:text-gray-700={activeView !== 'basic'}
            onclick={() => onChange('basic')}
            aria-pressed={activeView === 'basic'}
    >
        Basic
    </button>
    <button
            class="rounded border px-3 py-1 text-sm"
            class:bg-blue-600={activeView === 'dataset-metadata'}
            class:text-white={activeView === 'dataset-metadata'}
            class:border-blue-600={activeView === 'dataset-metadata'}
            class:bg-white={activeView !== 'dataset-metadata'}
            class:text-gray-700={activeView !== 'dataset-metadata'}
            onclick={() => onChange('dataset-metadata')}
            aria-pressed={activeView === 'dataset-metadata'}
    >
        Dataset Metadata
    </button>
    <button
            class="rounded border px-3 py-1 text-sm"
            class:bg-blue-600={activeView === 'file-metadata'}
            class:text-white={activeView === 'file-metadata'}
            class:border-blue-600={activeView === 'file-metadata'}
            class:bg-white={activeView !== 'file-metadata'}
            class:text-gray-700={activeView !== 'file-metadata'}
            onclick={() => onChange('file-metadata')}
            aria-pressed={activeView === 'file-metadata'}
    >
        File Metadata
    </button>
</div>
