<script lang="ts">
    import { page } from '$app/state';
    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {ProjectsRepository} from "$lib/data/ProjectsRepo";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import {isGuid} from "$lib/util/url-helper";
    import EntityHeader from "$lib/layout/EntityHeader.svelte";


    let projectId: string = page.params.project_id ?? '';
    if (!projectId) throw new Error('Collection ID is required');

    let projectsRepo = new ProjectsRepository(getOrFetchConfig().then(toApiConfig));

    let projectPromise = projectsRepo.getProjectByIdOrSlug(projectId);

    let title = 'RDPMS';
    projectPromise.then(project => {
        let slug = project.name ?? project.id ?? 'none';
        if (slug) {
            title = `${slug} - RDPMS`;
        }
    });
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
                <button class="rounded-md bg-gray-800 px-3 py-1.5 text-sm text-white hover:bg-black/90">
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

</main>
