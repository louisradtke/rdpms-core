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

    let collectionId: string = page.params.collection_id ?? '';
    let projectId: string = page.params.project_id ?? '';
    $: dataSetId = page.params.dataset_id ?? '';
    if (!collectionId) throw new Error('Collection ID is required');

    let collectionsRepo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
    let datasetsRepo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));

    let collectionReq = collectionsRepo.getCollectionByIdOrSlug(collectionId, projectId);
    let datasetsReq = collectionReq.then(c => datasetsRepo.listByCollection(c.id ?? ''));
    let datasetDetailedReq = isGuid(dataSetId)?
        datasetsRepo.getById(dataSetId) :
        datasetsReq.then(dsList => {
            let id = dsList.find(d => d.slug === dataSetId)?.id;
            if (!id) throw new Error(`Dataset with slug ${dataSetId} not found`);
            return datasetsRepo.getById(id);
        });

    let allDataSets = datasetsReq.then(dsl => dsl.map(ds => ({
        label: ds.name ?? ds.id ?? 'none',
        hrefValue: ds.slug ?? ds.id ?? 'none',
        tooltip: `data container with ID ${ds.id}`
    })));

    let title = 'RDPMS';
    collectionReq.then(summary => {
        let slug = summary.name ?? summary.id ?? 'none';
        if (slug) {
            title = `${slug} - RDPMS`;
        }
    });
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
                <div class="my-4"></div>

                {#if dsDetail.files}
                    <div class="space-y-6">
                        {#each dsDetail.files as file}
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