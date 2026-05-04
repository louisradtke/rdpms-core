<script lang="ts">
    import { page } from '$app/state';
    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {ProjectsRepository} from "$lib/data/ProjectsRepo";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";
    import {StoresRepository} from "$lib/data/StoresRepository";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import EntityHeader from "$lib/layout/EntityHeader.svelte";
    import type {CollectionSummaryDTO, DataStoreSummaryDTO, ProjectSummaryDTO} from "$lib/api_client";

    type StoreProperties = {
        endpointUrl?: string;
        bucket?: string;
        keyPrefix?: string;
        [key: string]: unknown;
    };

    let projectId: string = page.params.project_id ?? '';
    if (!projectId) throw new Error('Collection ID is required');

    const configPromise = getOrFetchConfig().then(toApiConfig);
    let projectsRepo = new ProjectsRepository(configPromise);
    let collectionsRepo = new CollectionsRepository(configPromise);
    let storesRepo = new StoresRepository(configPromise);

    let projectPromise = $state(projectsRepo.getProjectByIdOrSlug(projectId));

    const title = 'RDPMS';

    // Create collection modal state
    let isCreateOpen = $state(false);
    let creatingForProject = $state<{ id: string; name: string } | null>(null);
    let collectionForm = $state({
        name: "",
        slug: "",
        description: "",
        defaultDataStoreId: ""
    });
    let storesLoading = $state(false);
    let writableStores = $state<DataStoreSummaryDTO[]>([]);
    let createSaving = $state(false);
    let createErrorMsg = $state("");
    let copiedStoreId = $state<string | null>(null);
    let copyResetTimer: ReturnType<typeof setTimeout> | null = null;

    function toErrorMessage(e: unknown, fallback: string): string {
        if (typeof e === "string") return e.toUpperCase();
        if (e instanceof Error) return e.message ?? fallback;
        return fallback;
    }

    function sortCollections(collections: CollectionSummaryDTO[] | null | undefined): CollectionSummaryDTO[] {
        return [...(collections ?? [])].sort((left, right) =>
            (left.name ?? left.slug ?? left.id ?? '').localeCompare(
                right.name ?? right.slug ?? right.id ?? '',
                undefined,
                {numeric: true, sensitivity: 'base'}
            )
        );
    }

    function sortStores(stores: DataStoreSummaryDTO[] | null | undefined): DataStoreSummaryDTO[] {
        return [...(stores ?? [])].sort((left, right) =>
            (left.name ?? left.slug ?? left.id ?? '').localeCompare(
                right.name ?? right.slug ?? right.id ?? '',
                undefined,
                {numeric: true, sensitivity: 'base'}
            )
        );
    }

    function storeDisplayName(store: DataStoreSummaryDTO | null | undefined): string {
        return store?.name ?? store?.slug ?? store?.id ?? 'Unknown store';
    }

    function defaultStoreName(
        collection: CollectionSummaryDTO,
        stores: DataStoreSummaryDTO[] | null | undefined
    ): string {
        if (!collection.defaultDataStoreId) return 'no default store';
        const store = stores?.find((candidate) => candidate.id === collection.defaultDataStoreId);
        return storeDisplayName(store) ?? 'Unknown store';
    }

    function parseStoreProperties(store: DataStoreSummaryDTO): StoreProperties {
        if (!store.propertiesJson) return {};

        try {
            const parsed = JSON.parse(store.propertiesJson);
            if (parsed && typeof parsed === 'object' && !Array.isArray(parsed)) {
                return parsed as StoreProperties;
            }
        } catch {
            return {};
        }

        return {};
    }

    function formatBytes(value: number | null | undefined): string {
        const bytes = value ?? 0;
        if (bytes <= 0) return '0 B';

        const units = ['B', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB'];
        const exponent = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1);
        const amount = bytes / Math.pow(1024, exponent);
        const digits = amount >= 10 || exponent === 0 ? 0 : 1;
        return `${amount.toFixed(digits)} ${units[exponent]}`;
    }

    function formatReferenceCount(value: number | null | undefined): string {
        const count = value ?? 0;
        return `${count} reference${count === 1 ? '' : 's'}`;
    }

    async function copyStoreId(store: DataStoreSummaryDTO) {
        if (!store.id) return;

        const value = store.id;
        try {
            if (navigator.clipboard?.writeText) {
                await navigator.clipboard.writeText(value);
            } else {
                const fallback = document.createElement('textarea');
                fallback.value = value;
                fallback.setAttribute('readonly', 'true');
                fallback.style.position = 'absolute';
                fallback.style.left = '-9999px';
                document.body.appendChild(fallback);
                fallback.select();
                document.execCommand('copy');
                fallback.remove();
            }

            copiedStoreId = value;
            if (copyResetTimer) {
                clearTimeout(copyResetTimer);
            }
            copyResetTimer = setTimeout(() => {
                if (copiedStoreId === value) {
                    copiedStoreId = null;
                }
            }, 1500);
        } catch {
            copiedStoreId = null;
        }
    }

    async function openCreate(project: ProjectSummaryDTO) {
        const currentProjectId = project.id ?? "";
        if (!currentProjectId) {
            createErrorMsg = "Project id is missing.";
            return;
        }

        creatingForProject = { id: currentProjectId, name: project.name ?? project.slug ?? currentProjectId };
        collectionForm = {
            name: "",
            slug: "",
            description: "",
            defaultDataStoreId: ""
        };
        createErrorMsg = "";
        storesLoading = true;
        writableStores = [];
        isCreateOpen = true;

        try {
            writableStores = await storesRepo.listWritableByProject(currentProjectId);
        } catch (e) {
            createErrorMsg = toErrorMessage(e, "Failed to load writable stores.");
        } finally {
            storesLoading = false;
        }
    }

    function closeCreate() {
        isCreateOpen = false;
        creatingForProject = null;
        writableStores = [];
        createSaving = false;
        storesLoading = false;
        createErrorMsg = "";
    }

    async function saveCreateCollection() {
        if (!creatingForProject) return;
        if (!collectionForm.name.trim()) {
            createErrorMsg = "Collection name is required.";
            return;
        }

        createSaving = true;
        createErrorMsg = "";

        try {
            await collectionsRepo.createCollection({
                name: collectionForm.name.trim(),
                slug: collectionForm.slug.trim() || null,
                description: collectionForm.description.trim() || null,
                defaultDataStoreId: collectionForm.defaultDataStoreId || null,
                projectId: creatingForProject.id
            });

            projectPromise = projectsRepo.getProjectByIdOrSlug(projectId);
            closeCreate();
        } catch (e) {
            createErrorMsg = toErrorMessage(e, "Failed to create collection.");
        } finally {
            createSaving = false;
        }
    }
</script>

<svelte:head>
    <title>{title}</title>
</svelte:head>

<main class="container mx-auto px-2">
{#await projectPromise}
    <div class="mt-3 flex justify-center">
        <LoadingCircle/>
    </div>
{:then project}
    <EntityHeader type="PROJECT" entity={project} />

    <div class="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <!-- Left: Collections -->
        <section class="mt-2 lg:col-span-2">
            <div class="mb-3 flex items-center justify-between">
                <h2 class="text-lg font-semibold">Collections</h2>
                <button class="rounded-md bg-gray-800 px-3 py-1.5 text-sm text-white hover:bg-black/90" onclick={() => openCreate(project)}>
                    New collection
                </button>
            </div>

            <ul class="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
            {#each sortCollections(project.collections) as collection (collection.id)}
                    <li class="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
                        <div class="flex items-start justify-between gap-3">
                            <div>
                                <a href="/projects/{projectId}/c/{collection.slug ?? collection.id}"
                                   class="hover:no-underline">
                                    <h3 class="font-medium">{collection.name}</h3>
                                </a>
                                {#if collection.description}
                                    <p class="mt-1 text-sm text-gray-600 line-clamp-2">{collection.description}</p>
                                {/if}
                            </div>
                            <span class="shrink-0 rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-700">
                                {collection.dataSetCount + " dataset" + (collection.dataSetCount === 1 ? "" : "s")}
                            </span>
                        </div>
                        <p class="mt-3 text-sm text-gray-600">
                            Default store: <span class="font-medium text-gray-800">{defaultStoreName(collection, project.dataStores)}</span>
                        </p>
<!--                        <div class="mt-4 flex flex-wrap gap-2">-->
<!--                            <a href="#" class="rounded-md border border-gray-300 px-2.5 py-1 text-sm hover:bg-gray-50">Open</a>-->
<!--                            <button class="rounded-md border border-gray-300 px-2.5 py-1 text-sm hover:bg-gray-50">Preview</button>-->
<!--                            <button class="rounded-md border border-gray-300 px-2.5 py-1 text-sm hover:bg-gray-50">Run workflow</button>-->
<!--                        </div>-->
                    </li>
                {:else}
                    <p class="text-sm text-gray-600">
                        Project contains no collections
                    </p>
                {/each}
            </ul>

            <!-- Empty state (show when list is empty) -->
            <!--
            <div class="rounded-2xl border border-dashed border-gray-300 p-8 text-center text-gray-600">
              No collections yet. <button class="underline">Create one</button>.
            </div>
            -->
        </section>

        <!-- Right: Stores / Shares -->
        <aside class="lg:col-span-1 lg:sticky lg:top-4 h-fit">
            <h2 class="mb-3 text-lg font-semibold">Stores</h2>

            <div class="space-y-4">
                {#each sortStores(project.dataStores) as store (store.id)}
                    {@const properties = parseStoreProperties(store)}
                    <article class="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
                        <header class="mb-3 flex items-start justify-between gap-3">
                            <div class="min-w-0">
                                <h3 class="truncate font-medium" title={storeDisplayName(store)}>
                                    {storeDisplayName(store)}
                                </h3>
                                {#if store.slug}
                                    <p class="truncate text-xs text-gray-500" title={store.slug}>{store.slug}</p>
                                {/if}
                            </div>
                            <span class="shrink-0 rounded-full bg-gray-100 px-2 py-0.5 text-xs uppercase text-gray-700">
                                {store.storageType ?? 'store'}
                            </span>
                        </header>
                        <div class="mb-3 flex items-center justify-between gap-3">
                            <p class="min-w-0 truncate text-xs text-gray-500" title={store.id ?? ''}>
                                {store.id ?? 'no store id'}
                            </p>
                            <button
                                type="button"
                                class="inline-flex shrink-0 items-center gap-1 rounded-md border border-gray-300 px-2.5 py-1 text-xs font-medium text-gray-700 hover:bg-gray-50 disabled:cursor-default disabled:opacity-50"
                                onclick={() => copyStoreId(store)}
                                disabled={!store.id}
                                aria-label={store.id ? `Copy store id for ${storeDisplayName(store)}` : 'Store id unavailable'}
                                title={copiedStoreId === store.id ? 'Copied' : 'Copy store id'}
                            >
                                <svg aria-hidden="true" viewBox="0 0 20 20" fill="currentColor" class="size-4">
                                    <path d="M8 2a2 2 0 0 0-2 2v1H5a2 2 0 0 0-2 2v7a2 2 0 0 0 2 2h5a2 2 0 0 0 2-2v-1h1a2 2 0 0 0 2-2V6.5L11.5 2H8Zm0 1.5h3V5a1 1 0 0 0 1 1h1.5V9a.5.5 0 0 1-.5.5H12V8a2 2 0 0 0-2-2H7.5V4a.5.5 0 0 1 .5-.5Zm-3 4h5a.5.5 0 0 1 .5.5v7a.5.5 0 0 1-.5.5H5a.5.5 0 0 1-.5-.5v-7A.5.5 0 0 1 5 7.5Z"/>
                                </svg>
                                <span>{copiedStoreId === store.id ? 'Copied' : 'Copy ID'}</span>
                            </button>
                        </div>

                        <dl class="space-y-2 text-sm text-gray-700">
                            <div class="flex items-center justify-between gap-3">
                                <dt>Storage</dt>
                                <dd class="font-medium text-gray-900">{formatBytes(store.storageBytes)}</dd>
                            </div>
                            <div class="flex items-center justify-between gap-3">
                                <dt>References</dt>
                                <dd class="text-gray-900">{formatReferenceCount(store.storageReferenceCount ?? store.filesCount)}</dd>
                            </div>
                            <div class="flex items-center justify-between gap-3">
                                <dt>Access</dt>
                                <dd class="text-gray-900">{store.canWrite ? 'writable' : 'read-only'}</dd>
                            </div>
                            {#if properties.endpointUrl}
                                <div class="flex min-w-0 items-center justify-between gap-3">
                                    <dt class="shrink-0">Endpoint</dt>
                                    <dd class="min-w-0 truncate font-mono text-gray-900" title={properties.endpointUrl}>
                                        {properties.endpointUrl}
                                    </dd>
                                </div>
                            {/if}
                            {#if properties.bucket}
                                <div class="flex min-w-0 items-center justify-between gap-3">
                                    <dt class="shrink-0">Bucket</dt>
                                    <dd class="min-w-0 truncate font-mono text-gray-900" title={properties.bucket}>
                                        {properties.bucket}
                                    </dd>
                                </div>
                            {/if}
                            {#if properties.keyPrefix !== undefined}
                                <div class="flex min-w-0 items-center justify-between gap-3">
                                    <dt class="shrink-0">Prefix</dt>
                                    <dd class="min-w-0 truncate font-mono text-gray-900" title={properties.keyPrefix || '(root)'}>
                                        {properties.keyPrefix || '(root)'}
                                    </dd>
                                </div>
                            {/if}
                        </dl>
                    </article>
                {:else}
                    <p class="rounded-lg border border-dashed border-gray-300 p-4 text-sm text-gray-600">
                        Project contains no stores
                    </p>
                {/each}
            </div>
        </aside>
    </div>

    <!-- Loading state for whole page (optional) -->
    <!--
    <div class="mt-6 grid animate-pulse grid-cols-1 gap-6 lg:grid-cols-3">
      <div class="lg:col-span-2 space-y-4">
        <div class="h-28 rounded-2xl bg-gray-200"></div>
        <div class="h-28 rounded-2xl bg-gray-200"></div>
      </div>
      <div class="h-40 rounded-2xl bg-gray-200"></div>
    </div>
    -->

{:catch error}
    <p class="text-red-500">Error: {error.message}</p>
{/await}

{#if isCreateOpen}
    <!-- Backdrop -->
    <button class="fixed inset-0 bg-black/50 z-40" type="button" onclick={closeCreate} aria-label="Close create collection dialog"></button>

    <!-- Modal -->
    <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="w-full max-w-2xl rounded-lg bg-white shadow-lg">
            <div class="flex items-center justify-between border-b px-4 py-3">
                <h2 class="text-lg font-semibold">Create Collection</h2>
                <button class="text-gray-500 hover:text-gray-700" type="button" onclick={closeCreate} aria-label="Close">
                    ✕
                </button>
            </div>

            <form class="px-4 py-4" onsubmit={(evt) => { evt.preventDefault(); saveCreateCollection(); }}>
                <p class="mb-4 text-sm text-gray-600">
                    Project: <span class="font-medium">{creatingForProject?.name}</span>
                </p>

                <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
                    <div>
                        <label class="block text-sm font-medium mb-1" for="collection-name">Name</label>
                        <input
                                id="collection-name"
                                class="w-full rounded border px-3 py-2"
                                type="text"
                                bind:value={collectionForm.name}
                                required
                        />
                    </div>

                    <div>
                        <label class="block text-sm font-medium mb-1" for="collection-slug">Slug</label>
                        <input
                                id="collection-slug"
                                class="w-full rounded border px-3 py-2"
                                type="text"
                                bind:value={collectionForm.slug}
                        />
                    </div>

                    <div class="md:col-span-2">
                        <label class="block text-sm font-medium mb-1" for="collection-description">Description</label>
                        <textarea
                                id="collection-description"
                                class="w-full min-h-24 rounded border px-3 py-2"
                                bind:value={collectionForm.description}
                        ></textarea>
                    </div>

                    <div class="md:col-span-2">
                        <label class="block text-sm font-medium mb-1" for="collection-default-store">Default Data Store</label>
                        {#if storesLoading}
                            <div class="py-2"><LoadingCircle/></div>
                        {:else}
                            <select
                                    id="collection-default-store"
                                    class="w-full rounded border px-3 py-2"
                                    bind:value={collectionForm.defaultDataStoreId}
                            >
                                <option value="">None</option>
                                {#each writableStores as store (store.id)}
                                    <option value={store.id ?? ''}>
                                        {store.name ?? store.slug ?? store.id} ({store.slug ?? "no-slug"})
                                    </option>
                                {/each}
                            </select>
                            {#if writableStores.length === 0}
                                <p class="mt-1 text-sm text-amber-700">No writable stores found for this project.</p>
                            {/if}
                        {/if}
                    </div>
                </div>

                {#if createErrorMsg}
                    <p class="mt-4 text-sm text-red-600">{createErrorMsg}</p>
                {/if}

                <div class="mt-4 flex justify-end gap-2">
                    <button
                            type="button"
                            class="px-4 py-2 rounded border hover:bg-gray-50"
                            onclick={closeCreate}
                            disabled={createSaving}
                    >
                        Cancel
                    </button>
                    <button
                            type="submit"
                            class="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
                            disabled={createSaving || storesLoading}
                    >
                        {createSaving ? "Creating..." : "Create Collection"}
                    </button>
                </div>
            </form>
        </div>
    </div>
{/if}

</main>
