<script lang="ts">
    import {getApiConfig} from "$lib/util/config-helper";
    import {Configuration, ContainersApi, type ContainerSummaryDTO} from "$lib/api_client";

    // const conf = await getApiConfig();
    const conf = new Configuration({
        basePath: 'http://localhost:5000'
    })
    let containersApi = new ContainersApi(conf);

    let tableAccessors: Array<{visibleTitle: string, accessor: (container: ContainerSummaryDTO) => string}> = [
        {visibleTitle: 'ID', accessor: (container) => container.id ?? 'null'},
        {visibleTitle: 'Name', accessor: (container) => container.name ?? 'null'},
        {visibleTitle: '# Data Files', accessor: (container) => container.dataFilesCount?.toString() ?? '-1'},
        {visibleTitle: 'Default Data Store', accessor: (container) => container.defaultDataStoreId ?? 'null'},
    ]
</script>

<main class="container mx-auto">
    <h1 class="text-2xl font-bold my-4">Data Containers</h1>

    {#await containersApi.apiV1DataContainersGet()}
        <p>loading ...</p>
    {:then dataContainers}
        <table class="table-auto border-collapse border border-gray-300 w-full">
            <thead>
            <tr class="bg-gray-100">
                {#each tableAccessors.map(dct => dct.visibleTitle) as key}
                    <th class="border border-gray-300 px-4 py-2 text-left">{key}</th>
                {/each}
            </tr>
            </thead>
            <tbody>
            {#each dataContainers as container}
                <tr class="even:bg-gray-50">
                    <td class="border border-gray-300 px-4 py-2">
                        <a href="/data-containers/{container.id}">{container.id}</a>
                    </td>
                    {#each tableAccessors.slice(1).map(dct => dct.accessor(container)) as entry}
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
