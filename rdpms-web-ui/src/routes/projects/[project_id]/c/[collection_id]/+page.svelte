
<script lang="ts">
    import Sidebar from "$lib/layout/Sidebar.svelte";

    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";

    import { page } from '$app/state';
    import {DataSetsRepository} from "$lib/data/DataSetsRepository";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import EntityHeader from "$lib/layout/EntityHeader.svelte";

    // Reactive params
    let collectionSlug = $derived(page.params.collection_id ?? '');
    let projectSlug = $derived(page.params.project_id ?? '');

    // Validate required params
    $effect(() => {
        if (!collectionSlug) throw new Error('Collection ID is required');
    });

    // Re-create promises when params change
    let allCollections = $derived.by(async () => {
        const repo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
        let cl = await repo.getCollections({projectSlug: projectSlug});
        return cl.map(c => ({
            label: c.name ?? c.id ?? 'none',
            hrefValue: c.slug ?? c.id ?? 'none',
            tooltip: `data container with ID ${c.id}`
        }));
    });

    let collectionReq = $derived.by(() => {
        const repo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
        return repo.getCollectionByIdOrSlug(collectionSlug, projectSlug);
    });

    let datasetsReq = $derived.by(async () => {
        const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));
        let c = await collectionReq;
        return await repo.listByCollection(c.id ?? '');
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
</script>

<svelte:head>
    <title>{title}</title>
</svelte:head>

<div class="flex h-full">

    <Sidebar
        itemsPromise={allCollections}
        baseUrl="/projects/{projectSlug}/c/*"
    />

    <div class="flex flex-col flex-1">

        <main class="m-5 flex-1 overflow-y-auto p-4">

            {#await collectionReq}
                <LoadingCircle/>
            {:then collection}
                <EntityHeader type="COLLECTION" entity={collection} />
<!--                <span class="shrink-0 rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-700">-->
<!--                    {collection.dataSetCount + " dataset" + (collection.dataSetCount === 1 ? "" : "s")}-->
<!--                </span>-->

                <div class="my-4"></div>
            {:catch error}
                <p>{error}</p>
            {/await}

            <h2 class="text-2xl font-bold my-4">
                Contained datasets:
            </h2>

            <section class="table-section">
                {#await datasetsReq}
                    <div class="mt-3">
                        <LoadingCircle/>
                    </div>
                {:then datasets}

                <table class="table-fixed w-full">
                    <thead>
                    <tr>
                        <th class="text-left w-48">Dataset Name</th>
                        <th>Create date</th>
                        <th class="text-right w-10"></th>
                    </tr>
                    </thead>

                    <tbody>
                        {#each datasets as dataset (dataset.id)}
                            <tr>
                                <td class="text-left">
                                    <a href="/projects/{projectSlug}/c/{collectionSlug}/{dataset.slug ?? dataset.id}"
                                       class="text-blue-500 hover:underline">
                                        {dataset.name}
                                    </a>
                                <td>etc.</td>
                                <td class="text-right">
                                    <button
                                            class="px-3 py-1 rounded bg-blue-600 text-white hover:bg-blue-700"
                                            aria-label="Edit project"
                                    >
                                        Edit
                                    </button>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
                {:catch error}
                        <p class="text-center">Error: {error.message}</p>
                {/await}
            </section>
        </main>
    </div>
</div>