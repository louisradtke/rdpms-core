<script lang="ts">
    import { page } from "$app/state";
    import { getOrFetchConfig, toApiConfig } from "$lib/util/config-helper";
    import { CollectionsRepository } from "$lib/data/CollectionsRepository";
    import { DataSetsRepository } from "$lib/data/DataSetsRepository";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import EntityHeader from "$lib/layout/EntityHeader.svelte";
    import { isGuid } from "$lib/util/url-helper";
    import FileDisplay from "$lib/layout/FileDisplay.svelte";
    import { goto } from "$app/navigation";
    import { MetaDataRepository } from "$lib/data/MetaDataRepository";
    import {
        MetadataColumnTargetDTO,
        type DataSetSummaryDTO,
        type FileSummaryDTO
    } from "$lib/api_client";
    import type { VisualizationManifest } from "$lib/contracts/schemas/visualization-manifest.v1";
    import { FilesRepository } from "$lib/data/FilesRepository";
    import DatasetMetadataPanel from "$lib/components/datasets/DatasetMetadataPanel.svelte";
    import DatasetFilesBrowser from "$lib/components/datasets/DatasetFilesBrowser.svelte";
    import DatasetListControls from "$lib/components/collections/DatasetListControls.svelte";
    import {
        applyDatasetListQuery,
        parseDatasetListQuery,
        preserveDatasetListParams,
        writeDatasetListQuery,
        type DatasetListQuery
    } from "$lib/util/dataset-list-query";

    const VISUALIZATION_SCHEMA_ID = "urn:rdpms:core:schema:visualization-manifest:v1";
    const VISUALIZATION_KEY = "rdpms.viz";

    type DisplayItem = {
        itemId: string;
        title: string;
        file: FileSummaryDTO;
        preferredPluginIds: string[];
        preferredDefaultPluginId?: string;
        rendererOptions?: Record<string, unknown>;
        collapsible: boolean;
        collapsedByDefault: boolean;
    };

    type VisualizationViewState = {
        title?: string;
        items: DisplayItem[];
    } | null;

    // Reactive params
    let collectionId = $derived(page.params.collection_id ?? "");
    let projectId = $derived(page.params.project_id ?? "");
    let dataSetId = $derived(page.params.dataset_id ?? "");
    let reloadTick = $state(0);
    const datasetQuery = $derived(parseDatasetListQuery(page.url.searchParams));

    // Validate required params
    $effect(() => {
        if (!collectionId) throw new Error("Collection ID is required");
    });

    // Re-create promises when params change
    let collectionReq = $derived.by(() => {
        void reloadTick;
        const repo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
        return repo.getCollectionByIdOrSlug(collectionId, projectId);
    });

    let datasetsReq = $derived.by(async () => {
        void reloadTick;
        const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));
        let c = await collectionReq;
        return await repo.listByCollection(c.id ?? "");
    });

    let datasetDetailedReq = $derived.by(async () => {
        void reloadTick;
        const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));

        if (isGuid(dataSetId)) {
            return repo.getById(dataSetId);
        }

        let dsList = await datasetsReq;
        const id = dsList.find((d) => d.slug === dataSetId)?.id;
        if (!id) throw new Error(`Dataset with slug ${dataSetId} not found`);
        return repo.getById(id);
    });

    const normalize = (value?: string | null) => (value ?? "").toLowerCase();

    const isVisualizationSchema = (schemaId?: string | null): boolean => {
        const normalized = normalize(schemaId);
        return (
            normalized === VISUALIZATION_SCHEMA_ID ||
            normalized.includes("visualization-manifest.v1.schema.json")
        );
    };

    const parseManifest = (input: unknown): VisualizationManifest | null => {
        if (!input || typeof input !== "object") return null;
        const candidate = input as VisualizationManifest;
        if (!candidate.id || !Array.isArray(candidate.views)) return null;
        return candidate;
    };

    const findFileById = (fileId: string): Promise<FileSummaryDTO> | null => {
        const fielsRepo = new FilesRepository(getOrFetchConfig().then(toApiConfig));
        return fielsRepo.getById(fileId);
    };

    const mapManifestToDisplayItems = async (
        dataset: DataSetSummaryDTO,
        manifest: VisualizationManifest
    ): Promise<VisualizationViewState> => {
        const firstView = manifest.views?.[0];
        if (!firstView || !Array.isArray(firstView.items)) {
            return null;
        }

        const items: DisplayItem[] = [];
        for (const item of firstView.items) {
            if (!item.source?.fileId) continue;
            const file = await findFileById(item.source?.fileId);
            if (!file) continue;
            items.push({
                itemId: `${item.source.fileId}-${items.length}`,
                title: item.title ?? file.name ?? file.id ?? "file",
                file,
                preferredPluginIds: item.renderer?.kind ?? [],
                preferredDefaultPluginId: item.renderer?.default,
                rendererOptions:
                    (item.renderer?.options as Record<string, unknown> | undefined) ?? undefined,
                collapsible: item.collapsible ?? false,
                collapsedByDefault: item.collapsedByDefault ?? false
            });
        }

        if (items.length === 0) {
            return null;
        }

        return {
            title: firstView.title ?? manifest.title,
            items
        };
    };

    const getVisualizationView = async (
        dataset: DataSetSummaryDTO
    ): Promise<VisualizationViewState> => {
        const assignedViz = dataset.metaDates?.find(
            (entry) => normalize(entry.metadataKey) === VISUALIZATION_KEY
        );

        const metadataId = assignedViz?.metadataId;
        if (!metadataId) return null;

        const metadataRepo = new MetaDataRepository(getOrFetchConfig().then(toApiConfig));
        const metadata = await metadataRepo.getById(metadataId);
        const isValidated =
            metadata.validatedSchemas?.some((schema) => isVisualizationSchema(schema.schemaId)) ??
            false;
        if (!isValidated || !metadata.fileId) return null;

        const json = await metadataRepo.getJsonValueByFileId(metadata.fileId);
        const manifest = parseManifest(json);
        if (!manifest) return null;

        return mapManifestToDisplayItems(dataset, manifest);
    };

    let datasetPageReq = $derived.by(async () => {
        const dsDetail = await datasetDetailedReq;
        const visualization = await getVisualizationView(dsDetail);
        const collection = await collectionReq;
        const datasetColumns = (collection.metaDateColumns ?? []).filter(
            (column) => column.target === MetadataColumnTargetDTO.Dataset
        );
        return { dsDetail, visualization, datasetColumns };
    });

    let allDataSets = $derived.by(async () => {
        const datasets = await datasetsReq;
        const currentDataset = await datasetDetailedReq;
        const visibleDatasets = applyDatasetListQuery(datasets, datasetQuery);
        const currentIsVisible = visibleDatasets.some((ds) => ds.id === currentDataset.id);

        return {
            visibleDatasets,
            currentDataset: currentIsVisible ? null : currentDataset,
            totalCount: datasets.length
        };
    });

    const onDatasetMetadataChanged = () => {
        reloadTick += 1;
    };

    const setDatasetQuery = async (query: DatasetListQuery) => {
        const url = writeDatasetListQuery(page.url, query);
        await goto(url, { replaceState: true, keepFocus: true, noScroll: true });
    };

    const datasetHref = (dataset: DataSetSummaryDTO) => {
        const routeValue = dataset.slug ?? dataset.id ?? "";
        const url = new URL(
            `/projects/${projectId}/c/${collectionId}/${routeValue}`,
            window.location.origin
        );
        preserveDatasetListParams(url, page.url.searchParams);
        return `${url.pathname}${url.search}`;
    };

    const backToCollectionHref = $derived.by(() => {
        const url = new URL(`/projects/${projectId}/c/${collectionId}`, window.location.origin);
        url.searchParams.set("view", "basic");
        preserveDatasetListParams(url, page.url.searchParams);
        return `${url.pathname}${url.search}`;
    });

    let title = $derived(collectionId ? `${collectionId} - RDPMS` : "RDPMS");
</script>

<svelte:head>
    <title>{title}</title>
</svelte:head>

<aside class="w-72 shrink-0 overflow-y-auto border-r border-gray-200 bg-gray-100 p-4 text-gray-800">
    <a
        href={backToCollectionHref}
        class="mb-3 inline-flex rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm hover:bg-gray-50"
    >
        Back to collection
    </a>

    <DatasetListControls
        query={datasetQuery}
        onChange={setDatasetQuery}
        compact={true}
        collapsedByDefault={true}
    />

    <div class="mt-4">
        {#await allDataSets}
            <p class="text-sm text-gray-500">Loading datasets...</p>
        {:then sidebarData}
            <p class="mb-2 text-xs text-gray-500">
                Showing {sidebarData.visibleDatasets.length} of {sidebarData.totalCount}
            </p>

            {#if sidebarData.currentDataset}
                <div class="mb-3">
                    <p class="mb-1 text-xs font-semibold uppercase text-gray-500">
                        Current dataset
                    </p>
                    <a
                        href={datasetHref(sidebarData.currentDataset)}
                        class="block rounded-md bg-blue-50 px-3 py-2 text-sm text-blue-800"
                        title={sidebarData.currentDataset.id ?? ""}
                    >
                        {sidebarData.currentDataset.name ??
                            sidebarData.currentDataset.id ??
                            "Dataset"}
                    </a>
                </div>
            {/if}

            <ul class="space-y-1">
                {#each sidebarData.visibleDatasets as dataset (dataset.id)}
                    <li>
                        <a
                            href={datasetHref(dataset)}
                            class={[
                                "block rounded-md px-3 py-2 text-sm hover:bg-gray-300 focus:outline-none focus:ring focus:ring-gray-500 focus:ring-opacity-50",
                                (dataset.slug ?? dataset.id) === dataSetId
                                    ? "bg-gray-300 font-medium"
                                    : ""
                            ]}
                            title={dataset.id ?? ""}
                        >
                            {dataset.name ?? dataset.id ?? "Dataset"}
                        </a>
                    </li>
                {/each}
            </ul>
        {:catch error}
            <p class="mt-4 text-sm italic text-gray-500">
                Failed to load datasets: {error.message}
            </p>
        {/await}
    </div>
</aside>

<div class="flex min-w-0 flex-1 flex-col overflow-y-auto">
    <main class="m-5 min-w-0 flex-1 p-4">
        {#await datasetPageReq}
            <LoadingCircle />
        {:then pageData}
            <EntityHeader type="DATASET" entity={pageData.dsDetail} />
            <div class="my-6"></div>

            <DatasetMetadataPanel
                dataset={pageData.dsDetail}
                columns={pageData.datasetColumns}
                onDataChanged={onDatasetMetadataChanged}
            />

            <div class="my-6"></div>

            {#if pageData.visualization}
                <section class="mb-6">
                    <h2 class="text-lg font-semibold text-gray-800">
                        {pageData.visualization.title ?? "Visualization"}
                    </h2>
                </section>
                <div class="space-y-4">
                    {#each pageData.visualization.items as item (item.itemId)}
                        {#if item.collapsible}
                            <details
                                class="rounded-2xl border border-gray-200 bg-gray-50 p-3"
                                open={!item.collapsedByDefault}
                            >
                                <summary class="cursor-pointer text-sm font-medium text-gray-700"
                                    >{item.title}</summary
                                >
                                <div class="pt-3">
                                    <FileDisplay
                                        title={item.title}
                                        fileSlug={"file" + item.file.id}
                                        file={item.file}
                                        preferredPluginIds={item.preferredPluginIds}
                                        preferredDefaultPluginId={item.preferredDefaultPluginId}
                                        rendererOptions={item.rendererOptions}
                                        defaultDisplayMode="auto"
                                    />
                                </div>
                            </details>
                        {:else}
                            <FileDisplay
                                title={item.title}
                                fileSlug={"file" + item.file.id}
                                file={item.file}
                                preferredPluginIds={item.preferredPluginIds}
                                preferredDefaultPluginId={item.preferredDefaultPluginId}
                                rendererOptions={item.rendererOptions}
                                defaultDisplayMode="auto"
                            />
                        {/if}
                    {/each}
                </div>
            {/if}

            <DatasetFilesBrowser files={pageData.dsDetail.files ?? []} />
        {:catch error}
            <p>{error}</p>
        {/await}
    </main>
</div>
