<script lang="ts">
    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";
    import type {CollectionSummaryDTO} from "$lib/api_client";
    import {page} from "$app/state";

    let projectId: string = page.params.project_id ?? '';

    // const conf = await getApiConfig();
    let collectionsRepo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
    let collectionsPromise = collectionsRepo.getCollections({projectId: projectId});

    let tableAccessors: Array<{visibleTitle: string, accessor: (container: CollectionSummaryDTO) => string}> = [
        {visibleTitle: 'ID', accessor: (container) => container.id ?? 'null'},
        {visibleTitle: 'Name', accessor: (container) => container.name ?? 'null'},
        {visibleTitle: '# Data Sets', accessor: (container) => container.dataSetCount?.toString() ?? '-1'},
        {visibleTitle: 'Default Data Store', accessor: (container) => container.defaultDataStoreId ?? 'null'},
    ]
</script>

<main class="container mx-auto">
    <h1 class="text-2xl font-bold my-4">Data Collections</h1>

{#await collectionsPromise}
    <p>loading ...</p>
{:then collections}
    <table class="table-auto border-collapse border border-gray-300 w-full">
        <thead>
        <tr class="bg-gray-100">
            {#each tableAccessors.map(dct => dct.visibleTitle) as key (key)}
                <th class="border border-gray-300 px-4 py-2 text-left">{key}</th>
            {/each}
        </tr>
        </thead>
        <tbody>
        {#each collections as container (container.id)}
            <tr class="even:bg-gray-50">
                <td class="border border-gray-300 px-4 py-2">
                    <a href="/c/{container.id}">{container.id}</a>
                </td>
                {#each tableAccessors.slice(1).map(dct => dct.accessor(container)) as entry (entry)}
                    <td class="border border-gray-300 px-4 py-2">{entry}</td>
                {/each}
            </tr>
        {/each}
        </tbody>
    </table>
{:catch error}
    <p class="italic text-gray-500 mt-4">failed to load collections: ${error}</p>
{/await}

</main>
