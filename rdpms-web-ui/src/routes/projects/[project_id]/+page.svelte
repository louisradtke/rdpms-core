<script lang="ts">
    import { page } from '$app/state';
    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {ProjectsRepository} from "$lib/data/ProjectsRepo";
    import {CollectionsRepository} from "$lib/data/CollectionsRepository";
    import {StoresRepository} from "$lib/data/StoresRepository";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import EntityHeader from "$lib/layout/EntityHeader.svelte";
    import type { DataStoreSummaryDTO, ProjectSummaryDTO } from "$lib/api_client";


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

    function toErrorMessage(e: unknown, fallback: string): string {
        if (typeof e === "string") return e.toUpperCase();
        if (e instanceof Error) return e.message ?? fallback;
        return fallback;
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
            {#each project.collections ?? [] as collection (collection.id)}
                    <li class="rounded-2xl border border-gray-200 bg-white p-4 shadow-sm">
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
                <!-- Store card -->
                <article class="rounded-2xl border border-gray-200 bg-white p-4 shadow-sm">
                    <header class="mb-2 flex items-center justify-between">
                        <h3 class="font-medium">S3 Store 1</h3>
                        <span class="inline-flex items-center gap-1 rounded-full bg-emerald-50 px-2 py-0.5 text-xs text-emerald-700">
              <span class="h-2 w-2 rounded-full bg-emerald-600"></span> healthy
            </span>
                    </header>
                    <dl class="text-sm text-gray-700">
                        <div class="flex justify-between">
                            <dt>Bucket</dt><dd class="font-mono text-gray-800">rdpms-prod</dd>
                        </div>
                        <div class="flex justify-between">
                            <dt>Region</dt><dd>eu-central-1</dd>
                        </div>
                    </dl>
                    <div class="mt-3 flex gap-2">
                        <button class="rounded-md border border-gray-300 px-2.5 py-1 text-sm hover:bg-gray-50">Browse</button>
                        <button class="rounded-md border border-gray-300 px-2.5 py-1 text-sm hover:bg-gray-50">Sync</button>
                    </div>
                </article>

                <!-- Another store -->
                <article class="rounded-2xl border border-gray-200 bg-white p-4 shadow-sm">
                    <header class="mb-2 flex items-center justify-between">
                        <h3 class="font-medium">Share 1</h3>
                        <span class="inline-flex items-center gap-1 rounded-full bg-amber-50 px-2 py-0.5 text-xs text-amber-700">
              <span class="h-2 w-2 rounded-full bg-amber-500"></span> pending
            </span>
                    </header>
                    <p class="text-sm text-gray-700">Public link generation in progress…</p>
                </article>
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
