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

    let allDataSets = $derived.by(async () => {
        let dsl = await datasetsReq;
        return dsl.map(ds => ({
            label: ds.name ?? ds.id ?? 'none',
            hrefValue: ds.slug ?? ds.id ?? 'none',
            tooltip: `data container with ID ${ds.id}`
        }));
    });

    // Reactive title
    let title = $state('RDPMS');
    $effect(() => {
        collectionReq.then(summary => {
            const slug = summary.name ?? summary.id ?? 'none';
            if (slug) {
                title = `${slug} - RDPMS`;
            }
        });
    });

    // Construct base URL reactively
    let sidebarBaseUrl = $derived(`/projects/${projectId}/c/${collectionId}/*`);
</script>

<svelte:head>
    <title>{title}</title>
</svelte:head>

<div class="flex h-full">

    <Sidebar
            itemsPromise={allDataSets}
            baseUrl="/projects/{projectId}/c/{collectionId}/*"
    />

    <div class="flex flex-col flex-1">

        <main class="m-5 flex-1 overflow-y-auto p-4">

            {#await datasetDetailedReq}
                <LoadingCircle/>
            {:then dsDetail}
                <EntityHeader type="DATASET" entity={dsDetail} />
                <div class="my-6"></div>

                {#if dsDetail.files}
                    <div class="space-y-4">
                        {#each dsDetail.files as file (file.id)}
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
</div>