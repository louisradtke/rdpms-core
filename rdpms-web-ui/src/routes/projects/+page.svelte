<script lang="ts">
    import {getOrFetchConfig, toApiConfig} from "$lib/util/config-helper";
    import {ProjectsRepository} from "$lib/data/ProjectsRepo";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import type {ProjectSummaryDTO} from "$lib/api_client";

    const configPromise = getOrFetchConfig().then(toApiConfig);
    let projectsRepo = new ProjectsRepository(configPromise);
    let projectsPromise = $state(projectsRepo.getProjects());


    // Modal state
    let isEditOpen = $state(false);
    let editingProject = $state<{ id: string; name: string; slug?: string } | null>(null);
    let form = $state({ name: "", slug: "" });
    let saving = $state(false);
    let errorMsg = $state("");

    function toErrorMessage(e: unknown, fallback: string): string {
        if (typeof e === "string") return e.toUpperCase();
        if (e instanceof Error) return e.message ?? fallback;
        return fallback;
    }

    async function openEdit(project: ProjectSummaryDTO) {
        editingProject = { id: project.id ?? "", name: project.name ?? "", slug: project.slug ?? "" };
        form.name = project.name ?? "";
        form.slug = project.slug ?? "";
        errorMsg = "";
        isEditOpen = true;
    }

    function closeEdit() {
        isEditOpen = false;
        editingProject = null;
        errorMsg = "";
    }

    async function saveEdit() {
        if (!editingProject) return;
        saving = true;
        errorMsg = "";
        try {
            await projectsRepo.updateProject(editingProject.id, {
                name: form.name,
                slug: form.slug || null
            });
            // Refresh list by re-instantiating the promise (await block will re-run)
            projectsPromise = projectsRepo.getProjects();
            closeEdit();
        } catch (e) {
            errorMsg = toErrorMessage(e, "Failed to save");
        } finally {
            saving = false;
        }
    }

</script>

<svelte:head>
    <title>Projects - RDPMS</title>
</svelte:head>

<main class="container mx-auto px-2">
    <h1 class="text-2xl font-bold my-4">Projects</h1>

    <section class="table-section">
        <table class="table-fixed w-full">
            <thead>
            <tr>
                <th class="text-left w-48">Project Name</th>
                <th>etc.</th>
                <th class="text-right w-10"></th>
            </tr>
            </thead>

            <tbody>
            {#await projectsPromise}
                <tr>
                    <td colspan="3" class="py-8 text-center">
                        <LoadingCircle/>
                    </td>
                </tr>
            {:then projects}
                {#each projects as project (project.id)}
                    <tr>
                        <td class="text-left">
                            <a href="/projects/{project.slug ?? project.id}" class="text-blue-500 hover:underline">
                                {project.name}
                            </a>
                        </td>
                        <td>etc.</td>
                        <td class="text-right">
                            <button
                                    class="px-3 py-1 rounded bg-blue-600 text-white hover:bg-blue-700"
                                    onclick={() => openEdit(project)}
                                    aria-label="Edit project"
                            >
                                Edit
                            </button>
                        </td>
                    </tr>
                {/each}
            {:catch error}
                <tr>
                    <td colspan="3" class="text-center">Error: {error.message}</td>
                </tr>
            {/await}
            </tbody>
        </table>
    </section>

    {#if isEditOpen}
        <!-- Backdrop -->
        <button class="fixed inset-0 bg-black/50 z-40" type="button" onclick={closeEdit} aria-label="Close edit dialog"></button>

        <!-- Modal -->
        <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div class="w-full max-w-md rounded-lg bg-white shadow-lg">
                <div class="flex items-center justify-between border-b px-4 py-3">
                    <h2 class="text-lg font-semibold">Edit Project</h2>
                    <button class="text-gray-500 hover:text-gray-700" type="button" onclick={closeEdit} aria-label="Close">
                        ✕
                    </button>
                </div>

                <form class="px-4 py-4" onsubmit={(evt) => { evt.preventDefault(); saveEdit(); }}>
                    <div class="mb-4">
                        <label class="block text-sm font-medium mb-1" for="project-name">Name</label>
                        <input
                                id="project-name"
                                class="w-full rounded border px-3 py-2"
                                type="text"
                                bind:value={form.name}
                                required
                        />
                    </div>

                    <div class="mb-4">
                        <label class="block text-sm font-medium mb-1" for="project-slug">Slug (optional)</label>
                        <input
                                id="project-slug"
                                class="w-full rounded border px-3 py-2"
                                type="text"
                                bind:value={form.slug}
                        />
                    </div>

                    {#if errorMsg}
                        <p class="mb-3 text-sm text-red-600">{errorMsg}</p>
                    {/if}

                    <div class="flex justify-end gap-2">
                        <button
                                type="button"
                                class="px-4 py-2 rounded border hover:bg-gray-50"
                                onclick={closeEdit}
                                disabled={saving}
                        >
                            Cancel
                        </button>
                        <button
                                type="submit"
                                class="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
                                disabled={saving}
                        >
                            {saving ? "Saving..." : "Save"}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    {/if}
</main>
