<script lang="ts">
    import { MetadataColumnTargetDTO, type DataSetSummaryDTO, type MetaDateCollectionColumnDTO } from "$lib/api_client";
    import DataSetViewToggle from "$lib/components/collections/DataSetViewToggle.svelte";
    import DataSetBasicTable from "$lib/components/collections/DataSetBasicTable.svelte";
    import DataSetMetaCoverageTable from "$lib/components/collections/DataSetMetaCoverageTable.svelte";
    import DataSetFileMetaCoverageTable from "$lib/components/collections/DataSetFileMetaCoverageTable.svelte";

    type CollectionView = 'basic' | 'dataset-metadata' | 'file-metadata';
    let { datasets, columns, projectSlug, collectionSlug, activeView, onViewChange, onDelete } = $props<{
        datasets: DataSetSummaryDTO[];
        columns: MetaDateCollectionColumnDTO[];
        projectSlug: string;
        collectionSlug: string;
        activeView: CollectionView;
        onViewChange: (view: CollectionView) => void;
        onDelete: () => void;
    }>();

    const datasetColumns = $derived(columns.filter((c: MetaDateCollectionColumnDTO) => c.target === MetadataColumnTargetDTO.Dataset));
    const fileColumns = $derived(columns.filter((c: MetaDateCollectionColumnDTO) => c.target === MetadataColumnTargetDTO.File));
</script>

<DataSetViewToggle activeView={activeView} onChange={onViewChange} />

{#if activeView === 'basic'}
    <DataSetBasicTable
        datasets={datasets}
        projectSlug={projectSlug}
        collectionSlug={collectionSlug}
        onDelete={onDelete}
    />
{:else if activeView === 'dataset-metadata'}
    <DataSetMetaCoverageTable
        datasets={datasets}
        columns={datasetColumns}
        projectSlug={projectSlug}
        collectionSlug={collectionSlug}
    />
{:else}
    <DataSetFileMetaCoverageTable
        datasets={datasets}
        columns={fileColumns}
        projectSlug={projectSlug}
        collectionSlug={collectionSlug}
    />
{/if}
