<script lang="ts">
    let { showMetaTable, onChange } = $props<{
        showMetaTable: boolean;
        onChange: (view: 'basic' | 'meta') => void;
    }>();
</script>

<div class="mb-3 flex flex-wrap items-center gap-2">
    <span class="text-sm text-gray-600">View:</span>
    <button
            class="rounded border px-3 py-1 text-sm"
            class:bg-blue-600={!showMetaTable}
            class:text-white={!showMetaTable}
            class:border-blue-600={!showMetaTable}
            class:bg-white={showMetaTable}
            class:text-gray-700={showMetaTable}
            onclick={() => onChange('basic')}
            aria-pressed={!showMetaTable}
    >
        Basic
    </button>
    <button
            class="rounded border px-3 py-1 text-sm"
            class:bg-blue-600={showMetaTable}
            class:text-white={showMetaTable}
            class:border-blue-600={showMetaTable}
            class:bg-white={!showMetaTable}
            class:text-gray-700={!showMetaTable}
            onclick={() => onChange('meta')}
            aria-pressed={showMetaTable}
    >
        Metadata coverage
    </button>
</div>
