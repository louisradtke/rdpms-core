<script lang="ts">
    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";
    import type {CollectionSummaryDTO} from "$lib/api_client";
    import {page} from "$app/state";

    // Reactive project ID from URL params
    let projectId = $derived(page.params.project_id ?? '');

    // Re-fetch collections whenever projectId changes
    let collectionsPromise = $derived.by(() => {
        const repo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
        return repo.getCollections({projectId: projectId});
    });

    let tableAccessors: Array<{visibleTitle: string, accessor: (collection: CollectionSummaryDTO) => string}> = [
        {visibleTitle: 'ID', accessor: (collection) => collection.id ?? 'null'},
        {visibleTitle: 'Name', accessor: (collection) => collection.name ?? 'null'},
        {visibleTitle: '# Data Sets', accessor: (collection) => collection.dataSetCount?.toString() ?? '-1'},
        {visibleTitle: 'Default Data Store', accessor: (collection) => collection.defaultDataStoreId ?? 'null'},
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
        {#each collections as collection (collection.id)}
            <tr class="even:bg-gray-50">
                <td class="border border-gray-300 px-4 py-2">
                    <a href={`/projects/${projectId}/c/${collection.id}`}>{collection.id}</a>
                </td>
                {#each tableAccessors.slice(1).map(dct => dct.accessor(collection)) as entry (entry)}
                    <td class="border border-gray-300 px-4 py-2">{entry}</td>
                {/each}
            </tr>
        {/each}
        </tbody>
    </table>
{:catch error}
    <p class="italic text-gray-500 mt-4">failed to load collections: {error}</p>
{/await}

</main>