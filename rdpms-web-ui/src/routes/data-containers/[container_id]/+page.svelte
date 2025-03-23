<script lang="ts">
    import Sidebar from "$lib/layout/Sidebar.svelte";
    import {dataContainers} from "$lib/mock-items";
    import { page } from '$app/state';

    let containerId = page.params.container_id ?? '';
    let container = dataContainers.find(it => it.id === containerId);
</script>

<div class="flex h-full">

    <Sidebar
        items={dataContainers.map(it => ({
            name: it.name ?? 'none',
            hrefValue: it.id ?? 'none',
            label: `data container with ID ${it.id}`
        }))}
        baseUrl="/data-containers/*"
    />

    <div class="flex flex-col flex-1">

        <main class="flex-1 overflow-y-auto bg-gray-100 p-4">

            <h1>Here are the details about this container:</h1>

            {#if container}
                <table>
                    <thead>
                        <tr>
                            <th>Key</th>
                            <th>Value</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each Object.entries(container) as [key, value]}
                            <tr>
                                <td>{key}</td>
                                <td>{value}</td>
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