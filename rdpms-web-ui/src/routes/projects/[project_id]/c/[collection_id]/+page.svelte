
<script lang="ts">
    import Sidebar from "$lib/layout/Sidebar.svelte";

    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";

    import { page } from '$app/state';
    import { goto } from '$app/navigation';
    import {DataSetsRepository} from "$lib/data/DataSetsRepository";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import EntityHeader from "$lib/layout/EntityHeader.svelte";
    import type {AssignedMetaDateDTO, DataSetSummaryDTO, MetaDateCollectionColumnDTO} from "$lib/api_client";

    // Reactive params
    let collectionSlug = $derived(page.params.collection_id ?? '');
    let projectSlug = $derived(page.params.project_id ?? '');

    // Validate required params
    $effect(() => {
        if (!collectionSlug) throw new Error('Collection ID is required');
    });

    // Re-create promises when params change
    let allCollectionsReq = $derived.by(async () => {
        const repo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
        return repo.getCollections({projectSlug: projectSlug});
    });
    
    let collectionsSidebarListPromise = $derived.by(async () => {
        const cl = await allCollectionsReq;
        return cl.map(c => ({
            label: c.name ?? c.id ?? 'none',
            hrefValue: c.slug ?? c.id ?? 'none',
            tooltip: `data container with ID ${c.id}`
        }));
    })

    let collectionReq = $derived.by(async () => {
        const repo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
        const allCollections = await allCollectionsReq;
        const selectedCollectionId = allCollections.find(c => c.slug === collectionSlug)?.id;
        if (!selectedCollectionId) {
            throw new Error(`Collection ID ${collectionSlug} not found in project ${projectSlug}`);
        }
        return repo.getCollectionById(selectedCollectionId);
    });

    let datasetsReq = $derived.by(async () => {
        const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));
        let c = await collectionReq;
        return await repo.listByCollection(c.id ?? '');
    });

    let collectionAndDatasets = $derived.by(async () => {
        const [collection, datasets] = await Promise.all([collectionReq, datasetsReq]);
        return {collection, datasets};
    });

    let title = $derived.by(() => (collectionSlug ? `${collectionSlug} - RDPMS` : 'RDPMS'));

    const viewParam = $derived((page.url.searchParams.get('view') ?? 'basic').toLowerCase());
    const showMetaTable = $derived(viewParam === 'meta' || viewParam === 'metadata');

    const setViewParam = async (view: 'basic' | 'meta') => {
        const url = new URL(page.url);
        url.searchParams.set('view', view);
        await goto(url, { replaceState: true, keepFocus: true, noScroll: true });
    };

    const formatDate = (value?: Date | null) => value ? value.toLocaleString() : '—';
    const normalizeKey = (value?: string | null) => (value ?? '').toLowerCase();

    const findAssignedMeta = (dataset: DataSetSummaryDTO, column: MetaDateCollectionColumnDTO) => {
        return dataset.metaDates?.find((meta) => normalizeKey(meta.metadataKey) === normalizeKey(column.metadataKey));
    };

    const getColumnSchemaId = (column: MetaDateCollectionColumnDTO) => {
        return column.schema?.schemaId ?? column.schema?.id ?? null;
    };

    const isMetaValidated = (assigned: AssignedMetaDateDTO | undefined, column: MetaDateCollectionColumnDTO) => {
        const schemaId = getColumnSchemaId(column);
        if (!assigned || !schemaId) return false;
        return Boolean(assigned.field?.validatedSchemas?.some((schema) => (schema.schemaId ?? schema.id) === schemaId));
    };
</script>

<svelte:head>
    <title>{title}</title>
</svelte:head>

<div class="flex h-full">

    <Sidebar
        itemsPromise={collectionsSidebarListPromise}
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
                {#await collectionAndDatasets}
                    <div class="mt-3">
                        <LoadingCircle/>
                    </div>
                {:then data}
                <div class="mb-3 flex flex-wrap items-center gap-2">
                    <span class="text-sm text-gray-600">View:</span>
                    <button
                            class="rounded border px-3 py-1 text-sm"
                            class:bg-blue-600={!showMetaTable}
                            class:text-white={!showMetaTable}
                            class:border-blue-600={!showMetaTable}
                            class:bg-white={showMetaTable}
                            class:text-gray-700={showMetaTable}
                            onclick={() => setViewParam('basic')}
                            aria-pressed={!showMetaTable}
                    >
                        Basic
                    </button>
                    <button
                            class="rounded border px-3 py-1 text-sm"
                            class:bg-blue-600={showMetaTable}
                            class:text-white={showMetaTable}
                            class:border-blue-600={showMetaTable}
                            class:bg-white={!showMetaTable}
                            class:text-gray-700={!showMetaTable}
                            onclick={() => setViewParam('meta')}
                            aria-pressed={showMetaTable}
                    >
                        Metadata coverage
                    </button>
                </div>

                {#if !showMetaTable}
                    <table class="table-fixed w-full">
                        <thead>
                        <tr>
                            <th class="text-left w-48">Dataset Name</th>
                            <th>Create date</th>
                            <th>Begin</th>
                            <th>End</th>
                            <th>Files</th>
                            <th>Time series</th>
                            <th class="text-right w-10"></th>
                        </tr>
                        </thead>

                        <tbody>
                            {#each data.datasets as dataset (dataset.id)}
                                <tr>
                                    <td class="text-left">
                                        <a href="/projects/{projectSlug}/c/{collectionSlug}/{dataset.slug ?? dataset.id}"
                                           class="text-blue-500 hover:underline">
                                            {dataset.name}
                                        </a>
                                    </td>
                                    <td>{formatDate(dataset.createdStampUTC)}</td>
                                    <td>{formatDate(dataset.beginStampUTC)}</td>
                                    <td>{formatDate(dataset.endStampUTC)}</td>
                                    <td>{dataset.fileCount ?? 0}</td>
                                    <td>{dataset.isTimeSeries ? 'yes' : 'no'}</td>
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
                {:else}
                    <div class="mb-2 flex items-center gap-3 text-sm text-gray-600">
                        <span class="inline-flex items-center gap-1">
                            <span class="inline-block h-2 w-2 rounded-full bg-green-500"></span>
                            Validated
                        </span>
                        <span class="inline-flex items-center gap-1">
                            <span class="inline-block h-2 w-2 rounded-full bg-yellow-500"></span>
                            Set, not validated
                        </span>
                        <span class="inline-flex items-center gap-1">
                            <span class="inline-block h-2 w-2 rounded-full bg-gray-300"></span>
                            Missing
                        </span>
                    </div>
                    <div class="overflow-x-auto">
                        <table class="table-auto w-full">
                            <thead>
                            <tr>
                                <th class="text-left w-48">Dataset Name</th>
                                {#each data.collection.metaDateColumns ?? [] as column (column.metadataKey)}
                                    <th class="text-center min-w-[140px]" title={column.schema?.schemaId ?? column.schema?.id ?? ''}>
                                        {column.metadataKey ?? 'unknown'}
                                    </th>
                                {/each}
                            </tr>
                            </thead>
                            <tbody>
                                {#each data.datasets as dataset (dataset.id)}
                                    <tr>
                                        <td class="text-left">
                                            <a href="/projects/{projectSlug}/c/{collectionSlug}/{dataset.slug ?? dataset.id}"
                                               class="text-blue-500 hover:underline">
                                                {dataset.name}
                                            </a>
                                        </td>
                                        {#each data.collection.metaDateColumns ?? [] as column (column.metadataKey)}
                                            {@const assigned = findAssignedMeta(dataset, column)}
                                            {@const hasMeta = Boolean(assigned)}
                                            {@const validated = isMetaValidated(assigned, column)}
                                            <td class="text-center">
                                                <span
                                                    class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs"
                                                    class:bg-green-100={hasMeta && validated}
                                                    class:text-green-800={hasMeta && validated}
                                                    class:bg-yellow-100={hasMeta && !validated}
                                                    class:text-yellow-800={hasMeta && !validated}
                                                    class:bg-gray-100={!hasMeta}
                                                    class:text-gray-600={!hasMeta}
                                                    title={assigned?.metadataId ?? 'missing'}
                                                >
                                                    <span
                                                        class="inline-block h-2 w-2 rounded-full"
                                                        class:bg-green-500={hasMeta && validated}
                                                        class:bg-yellow-500={hasMeta && !validated}
                                                        class:bg-gray-300={!hasMeta}
                                                    ></span>
                                                    {#if hasMeta}
                                                        {validated ? 'valid' : 'set'}
                                                    {:else}
                                                        missing
                                                    {/if}
                                                </span>
                                            </td>
                                        {/each}
                                    </tr>
                                {/each}
                            </tbody>
                        </table>
                    </div>
                {/if}
                {:catch error}
                        <p class="text-center">Error: {error.message}</p>
                {/await}
            </section>
        </main>
    </div>
</div>
