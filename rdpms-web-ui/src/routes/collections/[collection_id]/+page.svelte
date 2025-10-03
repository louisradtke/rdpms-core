<script lang="ts">
    import Sidebar from "$lib/layout/Sidebar.svelte";

    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";

    import { page } from '$app/state';
    import {DataSetsRepository} from "$lib/data/DataSetsRepository";

    let collectionId: string = page.params.collection_id ?? '';
    if (!collectionId) throw new Error('Collection ID is required');

    let collectionsRepo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
    let datasetsRepo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));
    let allCollections = collectionsRepo.getCollections().then(cl => cl.map(c => ({
        label: c.name ?? c.id ?? 'none',
        hrefValue: c.id ?? 'none',
        tooltip: `data container with ID ${c.id}`
    })));
    let summaryReq = collectionsRepo.getCollectionById(collectionId);
    let datasetsReq = datasetsRepo.listByCollection(collectionId);
</script>

<svelte:head>
    <!--{#await summaryReq}-->
    <!--    &lt;!&ndash; &ndash;&gt;-->
    <!--{:then summary}-->
    <!--    <title>{summary.name ?? summary.id ?? 'none'} - RDPMS</title>-->
    <!--{:catch error}-->
        <title>RDPMS</title>
    <!--{/await}-->
</svelte:head>

<div class="flex h-full">

    <Sidebar
        itemsPromise={allCollections}
        baseUrl="/collections/*"
    />

    <div class="flex flex-col flex-1">

        <main class="m-5 flex-1 overflow-y-auto p-4">

            <h2 class="text-2xl font-bold my-4">
                Here are the details about this container:
            </h2>

            {#await summaryReq}
                <p>Loading...</p>
            {:then summary}
                <table class="table-auto border-collapse border border-gray-300 w-full">
                    <thead>
                        <tr class="bg-gray-100">
                            <th class="border border-gray-300 px-4 py-2 text-left">Key</th>
                            <th class="border border-gray-300 px-4 py-2 text-left">Value</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each Object.entries(summary) as [key, value] (key)}
                            <tr class="even:bg-gray-50">
                                <td class="border border-gray-300 px-4 py-2">{key}</td>
                                <td class="border border-gray-300 px-4 py-2">{value}</td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            {:catch error}
                <p>{error}</p>
            {/await}

            <h2 class="text-2xl font-bold my-4">
                Contained datasets:
            </h2>

            {#await datasetsReq}
                <p>Loading...</p>
            {:then datasets}
                <table class="table-auto border-collapse border border-gray-300 w-full">
                    <thead>
                        <tr class="bg-gray-100">
                            <th class="border border-gray-300 px-4 py-2 text-left">ID</th>
                            <th class="border border-gray-300 px-4 py-2 text-left">Name</th>
                            <th class="border border-gray-300 px-4 py-2 text-left">Size</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each datasets as dataset (dataset.id)}
                            <tr class="even:bg-gray-50">
                                <td class="border border-gray-300 px-4 py-2">{dataset.id}</td>
                                <td class="border border-gray-300 px-4 py-2">{dataset.name}</td>
                                <td class="border border-gray-300 px-4 py-2 text-right">{dataset.fileCount}</td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            {:catch error}
                <p>{error}</p>
            {/await}
        </main>
    </div>
</div>