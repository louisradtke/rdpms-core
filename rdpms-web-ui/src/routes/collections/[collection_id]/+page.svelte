<script lang="ts">
    import Sidebar from "$lib/layout/Sidebar.svelte";
    import {collections} from "$lib/mock-items";
    import { page } from '$app/state';

    let containerId = page.params.container_id ?? '';
    let container = collections.find(it => it.id === containerId);
</script>

<div class="flex h-full">

    <Sidebar
        items={collections.map(it => ({
            name: it.name ?? 'none',
            hrefValue: it.id ?? 'none',
            label: `data container with ID ${it.id}`
        }))}
        baseUrl="/collections/*"
    />

    <div class="flex flex-col flex-1">

        <main class="m-5 flex-1 overflow-y-auto p-4">

            <h1 class="text-2xl font-bold my-4">
                Here are the details about this container:
            </h1>

            {#if container}
                <table class="table-auto border-collapse border border-gray-300 w-full">
                    <thead>
                        <tr class="bg-gray-100">
                            <th class="border border-gray-300 px-4 py-2 text-left">Key</th>
                            <th class="border border-gray-300 px-4 py-2 text-left">Value</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each Object.entries(container) as [key, value] (key)}
                            <tr class="even:bg-gray-50">
                                <td class="border border-gray-300 px-4 py-2">{key}</td>
                                <td class="border border-gray-300 px-4 py-2">{value}</td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            {:else}
                <p>No container found with the given ID.</p>
            {/if}
        </main>
    </div>
</div>