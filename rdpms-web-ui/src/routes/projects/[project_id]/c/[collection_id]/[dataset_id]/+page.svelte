<script lang="ts">

    import { page } from '$app/state';
    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";
    import {DataSetsRepository} from "$lib/data/DataSetsRepository";
    import Sidebar from "$lib/layout/Sidebar.svelte";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import EntityHeader from "$lib/layout/EntityHeader.svelte";
    import {isGuid} from "$lib/util/url-helper";
    import FileDisplay from "$lib/layout/FileDisplay.svelte";
    import {MetaDataRepository} from "$lib/data/MetaDataRepository";
    import type {DataSetDetailedDTO, FileSummaryDTO} from "$lib/api_client";
    import type {VisualizationManifest} from "$lib/contracts/schemas/visualization-manifest.v1";

    const VISUALIZATION_SCHEMA_ID = "urn:rdpms:core:schema:visualization-manifest:v1";
    const VISUALIZATION_KEY = "viz";

    type DisplayItem = {
        itemId: string;
        title: string;
        file: FileSummaryDTO;
        preferredPluginIds: string[];
        preferredDefaultPluginId?: string;
        collapsible: boolean;
        collapsedByDefault: boolean;
    };

    type VisualizationViewState = {
        title?: string;
        items: DisplayItem[];
    } | null;

    // Reactive params
    let collectionId = $derived(page.params.collection_id ?? '');
    let projectId = $derived(page.params.project_id ?? '');
    let dataSetId = $derived(page.params.dataset_id ?? '');

    // Validate required params
    $effect(() => {
        if (!collectionId) throw new Error('Collection ID is required');
    });

    // Re-create promises when params change
    let collectionReq = $derived.by(() => {
        const repo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
        return repo.getCollectionByIdOrSlug(collectionId, projectId);
    });

    let datasetsReq = $derived.by(async () => {
        const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));
        let c = await collectionReq;
        return await repo.listByCollection(c.id ?? '');
    });

    let datasetDetailedReq = $derived.by(async () => {
        const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));

        if (isGuid(dataSetId)) {
            return repo.getById(dataSetId);
        }

        let dsList = await datasetsReq;
        const id = dsList.find(d => d.slug === dataSetId)?.id;
        if (!id) throw new Error(`Dataset with slug ${dataSetId} not found`);
        return repo.getById(id);
    });

    const normalize = (value?: string | null) => (value ?? "").toLowerCase();

    const isVisualizationSchema = (schemaId?: string | null): boolean => {
        const normalized = normalize(schemaId);
        return normalized === VISUALIZATION_SCHEMA_ID || normalized.includes("visualization-manifest.v1.schema.json");
    };

    const parseManifest = (input: unknown): VisualizationManifest | null => {
        if (!input || typeof input !== "object") return null;
        const candidate = input as VisualizationManifest;
        if (!candidate.id || !Array.isArray(candidate.views)) return null;
        return candidate;
    };

    const findFileById = (files: FileSummaryDTO[] | null | undefined, fileId: string): FileSummaryDTO | null => {
        if (!files || !fileId) return null;
        return files.find((f) => f.id === fileId) ?? null;
    };

    const mapManifestToDisplayItems = (dataset: DataSetDetailedDTO, manifest: VisualizationManifest): VisualizationViewState => {
        const firstView = manifest.views?.[0];
        if (!firstView || !Array.isArray(firstView.items)) {
            return null;
        }

        const items: DisplayItem[] = [];
        for (const item of firstView.items) {
            const file = findFileById(dataset.files, item.source?.fileId ?? "");
            if (!file) continue;
            items.push({
                itemId: `${item.source.fileId}-${items.length}`,
                title: item.title ?? file.name ?? file.id ?? "file",
                file,
                preferredPluginIds: item.renderer?.kind ?? [],
                preferredDefaultPluginId: item.renderer?.default,
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

    const getVisualizationView = async (dataset: DataSetDetailedDTO): Promise<VisualizationViewState> => {
        const assignedViz = dataset.metaDates?.find(
            (entry) => normalize(entry.metadataKey) === VISUALIZATION_KEY
        );

        const metadataId = assignedViz?.metadataId;
        if (!metadataId) return null;

        const metadataRepo = new MetaDataRepository(getOrFetchConfig().then(toApiConfig));
        const metadata = await metadataRepo.getById(metadataId);
        const isValidated = metadata.validatedSchemas?.some((schema) => isVisualizationSchema(schema.schemaId)) ?? false;
        if (!isValidated || !metadata.fileId) return null;

        const json = await metadataRepo.getJsonValueByFileId(metadata.fileId);
        const manifest = parseManifest(json);
        if (!manifest) return null;

        return mapManifestToDisplayItems(dataset, manifest);
    };

    let datasetPageReq = $derived.by(async () => {
        const dsDetail = await datasetDetailedReq;
        const visualization = await getVisualizationView(dsDetail);
        return {dsDetail, visualization};
    });

    let allDataSets = $derived.by(async () => {
        let datasets = await datasetsReq;
        datasets = datasets.filter(ds => ds.lifecycleState?.toLowerCase() === 'sealed');
        datasets.sort((a, b) => {
            if (a.createdStampUTC && b.createdStampUTC) {
                return b.createdStampUTC.getTime() - a.createdStampUTC.getTime();
            }
            if (a.createdStampUTC) return 1;
            if (b.createdStampUTC) return -1;
            return 0;
        });
        return datasets.map(ds => ({
            label: ds.name ?? ds.id ?? 'none',
            hrefValue: ds.slug ?? ds.id ?? 'none',
            tooltip: `data set with ID ${ds.id}`
        }));
    });

    let title = $derived(collectionId ? `${collectionId} - RDPMS` : "RDPMS");
</script>

<svelte:head>
    <title>{title}</title>
</svelte:head>

<Sidebar
        itemsPromise={allDataSets}
        baseUrl="/projects/{projectId}/c/{collectionId}/*"
/>

<div class="flex flex-col flex-1 overflow-y-auto">

    <main class="m-5 flex-1 p-4">

        {#await datasetPageReq}
            <LoadingCircle/>
        {:then pageData}
            <EntityHeader type="DATASET" entity={pageData.dsDetail} />
            <div class="my-6"></div>

            {#if pageData.visualization}
                <section class="mb-6">
                    <h2 class="text-lg font-semibold text-gray-800">{pageData.visualization.title ?? "Visualization"}</h2>
                </section>
                <div class="space-y-4">
                    {#each pageData.visualization.items as item (item.itemId)}
                        {#if item.collapsible}
                            <details class="rounded-2xl border border-gray-200 bg-gray-50 p-3" open={!item.collapsedByDefault}>
                                <summary class="cursor-pointer text-sm font-medium text-gray-700">{item.title}</summary>
                                <div class="pt-3">
                                    <FileDisplay
                                        title={item.title}
                                        fileSlug={"file" + item.file.id}
                                        file={item.file}
                                        preferredPluginIds={item.preferredPluginIds}
                                        preferredDefaultPluginId={item.preferredDefaultPluginId}
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
                            />
                        {/if}
                    {/each}
                </div>
            {:else if pageData.dsDetail.files}
                <div class="space-y-4">
                    {#each pageData.dsDetail.files as file (file.id)}
                        <FileDisplay
                            title={file.name ?? file.id ?? 'none'}
                            fileSlug={"file" + file.id}
                            file={file}/>
                    {/each}
                </div>
            {/if}
        {:catch error}
            <p>{error}</p>
        {/await}

    </main>
</div>
