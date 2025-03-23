<script lang="ts">
    import type { SidebarItem } from '$lib/SidebarItem';

    export let items: SidebarItem[]; // List of sidebar items
    export let baseUrl: string; // URL or partial link containing a wildcard for hrefValue

    // Utility function to construct the final href for each item
    const constructHref = (hrefValue: string): string => {
        return baseUrl.replace('*', hrefValue);
    };
</script>

<aside class="w-64 bg-gray-800 text-white p-4 space-y-2">
    <ul class="space-y-1">
        {#each items.map((v, i) => ({item: v, index: i})) as vi (vi.index)}
            <li>
                <a
                        href="{constructHref(vi.item.hrefValue)}"
                        class="block px-3 py-2 rounded-md hover:bg-gray-700 focus:outline-none focus:ring focus:ring-gray-500 focus:ring-opacity-50"
                        title={vi.item.tooltip ?? ''}
                >
                    {vi.item.label}
                </a>
            </li>
        {/each}
    </ul>
</aside>