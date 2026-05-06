<script lang="ts">
    import {
        MetadataColumnTargetDTO,
        type DataSetSummaryDTO,
        type MetaDateCollectionColumnDTO
    } from "$lib/api_client";
    import DatasetListControls from "$lib/components/collections/DatasetListControls.svelte";
    import DataSetViewToggle from "$lib/components/collections/DataSetViewToggle.svelte";
    import DataSetBasicTable from "$lib/components/collections/DataSetBasicTable.svelte";
    import DataSetMetaCoverageTable from "$lib/components/collections/DataSetMetaCoverageTable.svelte";
    import DataSetFileMetaCoverageTable from "$lib/components/collections/DataSetFileMetaCoverageTable.svelte";
    import { applyDatasetListQuery, type DatasetListQuery } from "$lib/util/dataset-list-query";

    type CollectionView = "basic" | "dataset-metadata" | "file-metadata";
    let {
        datasets,
        columns,
        projectSlug,
        collectionSlug,
        activeView,
        datasetQuery,
        searchParams,
        onDatasetQueryChange,
        onViewChange,
        onDelete,
        onDataChanged
    } = $props<{
        datasets: DataSetSummaryDTO[];
        columns: MetaDateCollectionColumnDTO[];
        projectSlug: string;
        collectionSlug: string;
        activeView: CollectionView;
        datasetQuery: DatasetListQuery;
        searchParams: URLSearchParams;
        onDatasetQueryChange: (query: DatasetListQuery) => void;
        onViewChange: (view: CollectionView) => void;
        onDelete: () => void;
        onDataChanged: () => void;
    }>();

    const datasetColumns = $derived(
        columns.filter(
            (c: MetaDateCollectionColumnDTO) => c.target === MetadataColumnTargetDTO.Dataset
        )
    );
    const fileColumns = $derived(
        columns.filter(
            (c: MetaDateCollectionColumnDTO) => c.target === MetadataColumnTargetDTO.File
        )
    );
    const visibleDatasets = $derived(applyDatasetListQuery(datasets, datasetQuery));
</script>

<DataSetViewToggle {activeView} onChange={onViewChange} />

<div class="my-3">
    <DatasetListControls query={datasetQuery} onChange={onDatasetQueryChange} />
</div>

{#if activeView === "basic"}
    <DataSetBasicTable
        datasets={visibleDatasets}
        {projectSlug}
        {collectionSlug}
        query={datasetQuery}
        {searchParams}
        totalCount={datasets.length}
        onQueryChange={onDatasetQueryChange}
        {onDelete}
    />
{:else if activeView === "dataset-metadata"}
    <DataSetMetaCoverageTable
        datasets={visibleDatasets}
        columns={datasetColumns}
        {projectSlug}
        {collectionSlug}
        {searchParams}
        {onDataChanged}
    />
{:else}
    <DataSetFileMetaCoverageTable
        datasets={visibleDatasets}
        columns={fileColumns}
        {projectSlug}
        {collectionSlug}
        {searchParams}
        {onDataChanged}
    />
{/if}
