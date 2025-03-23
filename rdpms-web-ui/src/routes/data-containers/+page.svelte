<script lang="ts">
    import {dataContainers} from "$lib/mock-items";
</script>

<h1 class="text-2xl font-bold my-4">Data Containers</h1>

{#if dataContainers}
    <table class="table-auto border-collapse border border-gray-300 w-full">
        <thead>
        <tr class="bg-gray-100">
            {#each Object.keys(dataContainers[0]) as key}
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
                {#each Object.keys(container).slice(1) as key}
                    <td class="border border-gray-300 px-4 py-2">{container[key]}</td>
                {/each}
            </tr>
        {/each}
        </tbody>
    </table>
{:else}
    <p class="italic text-gray-500 mt-4">there are no containers ...</p>
{/if}