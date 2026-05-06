<script lang="ts">
    import type { SidebarItem } from "$lib/SidebarItem";

    let { itemsPromise, baseUrl } = $props<{
        itemsPromise: Promise<SidebarItem[]>;
        baseUrl: string;
    }>();

    const constructHref = (hrefValue: string): string => {
        return baseUrl.replace("*", hrefValue);
    };
</script>

<aside
    class="w-64 shrink-0 bg-gray-100 text-gray-800 border-gray-400 p-4 space-y-2 overflow-y-auto"
>
    <ul class="space-y-1">
        {#await itemsPromise}
            <li>Loading...</li>
        {:then items}
            {#each items as item, index (index)}
                <li>
                    <a
                        href={constructHref(item.hrefValue)}
                        class="block px-3 py-2 rounded-md hover:bg-gray-300 focus:outline-none focus:ring
                            focus:ring-gray-500 focus:ring-opacity-50"
                        title={item.tooltip ?? ""}
                    >
                        {item.label}
                    </a>
                </li>
            {/each}
        {:catch error}
            <p class="italic text-gray-500 mt-4">failed to load collections: ${error}</p>
        {/await}
    </ul>
</aside>
