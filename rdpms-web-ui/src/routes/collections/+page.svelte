<script lang="ts">
    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";
    import type {CollectionSummaryDTO} from "$lib/api_client";

    // const conf = await getApiConfig();
    let containersRepo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));

    let tableAccessors: Array<{visibleTitle: string, accessor: (container: CollectionSummaryDTO) => string}> = [
        {visibleTitle: 'ID', accessor: (container) => container.id ?? 'null'},
        {visibleTitle: 'Name', accessor: (container) => container.name ?? 'null'},
        {visibleTitle: '# Data Files', accessor: (container) => container.dataFilesCount?.toString() ?? '-1'},
        {visibleTitle: 'Default Data Store', accessor: (container) => container.defaultDataStoreId ?? 'null'},
    ]
</script>

<main class="container mx-auto">
    <h1 class="text-2xl font-bold my-4">Data Containers</h1>

    {#await containersRepo.getCollections()}
        <p>loading ...</p>
    {:then dataContainers}
        <table class="table-auto border-collapse border border-gray-300 w-full">
            <thead>
            <tr class="bg-gray-100">
                {#each tableAccessors.map(dct => dct.visibleTitle) as key (key)}
                    <th class="border border-gray-300 px-4 py-2 text-left">{key}</th>
                {/each}
            </tr>
            </thead>
            <tbody>
            {#each dataContainers as container (container.id)}
                <tr class="even:bg-gray-50">
                    <td class="border border-gray-300 px-4 py-2">
                        <a href="/src/routes/collections/{container.id}">{container.id}</a>
                    </td>
                    {#each tableAccessors.slice(1).map(dct => dct.accessor(container)) as entry (entry)}
                        <td class="border border-gray-300 px-4 py-2">{entry}</td>
                    {/each}
                </tr>
            {/each}
            </tbody>
        </table>
    {:catch error}
        <p class="italic text-gray-500 mt-4">failed to load containers ${error}</p>
    {/await}

</main>
