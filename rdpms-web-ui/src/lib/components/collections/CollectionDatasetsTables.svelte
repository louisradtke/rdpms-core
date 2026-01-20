<script lang="ts">
    import type { DataSetSummaryDTO, MetaDateCollectionColumnDTO } from "$lib/api_client";
    import DataSetViewToggle from "$lib/components/collections/DataSetViewToggle.svelte";
    import DataSetBasicTable from "$lib/components/collections/DataSetBasicTable.svelte";
    import DataSetMetaCoverageTable from "$lib/components/collections/DataSetMetaCoverageTable.svelte";

    let { datasets, columns, projectSlug, collectionSlug, showMetaTable, onViewChange } = $props<{
        datasets: DataSetSummaryDTO[];
        columns: MetaDateCollectionColumnDTO[];
        projectSlug: string;
        collectionSlug: string;
        showMetaTable: boolean;
        onViewChange: (view: 'basic' | 'meta') => void;
    }>();
</script>

<DataSetViewToggle showMetaTable={showMetaTable} onChange={onViewChange} />

{#if !showMetaTable}
    <DataSetBasicTable
        datasets={datasets}
        projectSlug={projectSlug}
        collectionSlug={collectionSlug}
    />
{:else}
    <DataSetMetaCoverageTable
        datasets={datasets}
        columns={columns}
        projectSlug={projectSlug}
        collectionSlug={collectionSlug}
    />
{/if}
