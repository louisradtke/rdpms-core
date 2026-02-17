
<script lang="ts">
    import Sidebar from "$lib/layout/Sidebar.svelte";

    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";

    import { page } from '$app/state';
    import { goto } from '$app/navigation';
    import {DataSetsRepository} from "$lib/data/DataSetsRepository";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import EntityHeader from "$lib/layout/EntityHeader.svelte";
    import CollectionDatasetsTables from "$lib/components/collections/CollectionDatasetsTables.svelte";

    // Reactive params
    let collectionSlug = $derived(page.params.collection_id ?? '');
    let projectSlug = $derived(page.params.project_id ?? '');
    let reloadTick = $state(0);

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

    type CollectionView = 'basic' | 'dataset-metadata' | 'file-metadata';
    const viewParam = $derived((page.url.searchParams.get('view') ?? 'basic').toLowerCase());
    const activeView = $derived.by((): CollectionView => {
        if (viewParam === 'dataset-metadata' || viewParam === 'meta' || viewParam === 'metadata') {
            return 'dataset-metadata';
        }
        if (viewParam === 'file-metadata' || viewParam === 'meta-files') {
            return 'file-metadata';
        }
        return 'basic';
    });
    let currentView = $derived(activeView);

    let datasetsReq = $derived.by(async () => {
        void reloadTick; // make $derived.by read it as dependency
        const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));
        let c = await collectionReq;
        const view = currentView;
        if (view === 'dataset-metadata') {
            return await repo.listByCollection(c.id ?? '', { view: 'metadata', metadataTarget: 'dataset' });
        }
        if (view === 'file-metadata') {
            return await repo.listByCollection(c.id ?? '', { view: 'metadata', metadataTarget: 'file' });
        }
        return await repo.listByCollection(c.id ?? '', { view: 'summary' });
    });

    let collectionAndDatasets = $derived.by(async () => {
        const [collection, datasets] = await Promise.all([collectionReq, datasetsReq]);
        return {collection, datasets};
    });

    let title = $derived.by(() => (collectionSlug ? `${collectionSlug} - RDPMS` : 'RDPMS'));

    const setViewParam = async (view: CollectionView) => {
        if (view === currentView) return;
        currentView = view;
        reloadTick += 1;
        const url = new URL(page.url);
        url.searchParams.set('view', view);
        await goto(url, { replaceState: true, keepFocus: true, noScroll: true });
    };

    const onDatasetDeleted = () => {
        reloadTick += 1;
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
                <div class="mt-2">
                    <a
                        class="inline-flex rounded-md border border-gray-300 px-3 py-1 text-sm hover:bg-gray-50"
                        href="/projects/{projectSlug}/c/{collectionSlug}/-"
                    >
                        Settings
                    </a>
                </div>
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
                <CollectionDatasetsTables
                    datasets={data.datasets}
                    columns={data.collection.metaDateColumns ?? []}
                    projectSlug={projectSlug}
                    collectionSlug={collectionSlug}
                    activeView={currentView}
                    onViewChange={setViewParam}
                    onDelete={onDatasetDeleted}
                    onDataChanged={onDatasetDeleted}
                />
                {:catch error}
                        <p class="text-center">Error: {error.message}</p>
                {/await}
            </section>
        </main>
    </div>
</div>
