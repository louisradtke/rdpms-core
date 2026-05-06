<script lang="ts">
    import SvelteTable from "svelte-table";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import type { CorePluginPolicy } from "$lib/layout/displayPlugins/plugin-registry";

    let {
        dataUri,
        policy
    }: {
        dataUri: string;
        policy?: CorePluginPolicy;
    } = $props();

    interface Row {
        [key: string]: string | number;
    }

    interface Column {
        key: string;
        title: string;
        value?: (value: any) => string;
        sortable: boolean;
    }

    interface TableData {
        rows: Row[];
        columns: Column[];
    }

    const DEFAULT_MAX_SIZE = 10 * 1024 * 1024;
    const DEFAULT_PREVIEW_ROWS = 500;

    let showAllRows = $state(false);
    let maxSize = $derived(policy?.display?.maxBytes ?? DEFAULT_MAX_SIZE);
    let rowPreviewLimit = $derived(policy?.display?.rowPreviewLimit ?? DEFAULT_PREVIEW_ROWS);

    async function fetchAndParseCSV(): Promise<TableData> {
        const response = await fetch(dataUri);

        if (!response.ok) {
            throw new Error(`Failed to fetch data: ${response.statusText}`);
        }

        // Check content length
        const contentLength = response.headers.get("content-length");
        if (contentLength && parseInt(contentLength) > maxSize) {
            throw new Error(
                policy?.display?.oversizeMessage ??
                    `File size exceeds ${(maxSize / 1024 / 1024).toFixed(0)} MiB limit`
            );
        }

        const text = await response.text();

        // Additional size check after download
        const textSize = new Blob([text]).size;
        if (textSize > maxSize) {
            throw new Error(
                policy?.display?.oversizeMessage ??
                    `File size exceeds ${(maxSize / 1024 / 1024).toFixed(0)} MiB limit`
            );
        }

        // Parse CSV
        const lines = text.trim().split("\n");
        if (lines.length === 0) {
            throw new Error("CSV file is empty");
        }

        // Parse header row
        const headers = lines[0].split(",").map((h) => h.trim().replace(/^"|"$/g, ""));

        // Create columns
        const columns: Column[] = headers.map((header) => ({
            key: header,
            title: header,
            value: (v: any) => v[header] || "",
            sortable: true
        }));

        // Parse data rows
        const rows: Row[] = lines
            .slice(1)
            .filter((line) => line.trim())
            .map((line) => {
                const values = line.split(",").map((v) => v.trim().replace(/^"|"$/g, ""));
                const row: Row = {};
                headers.forEach((header, index) => {
                    const value = values[index] || "";
                    // Try to convert to number if possible
                    const numValue = parseFloat(value);
                    row[header] = isNaN(numValue) ? value : numValue;
                });
                return row;
            });

        return { rows, columns };
    }

    let tableDataPromise = $derived(fetchAndParseCSV());
</script>

{#await tableDataPromise}
    <div class="flex justify-center items-center">
        <LoadingCircle />
    </div>
{:then { rows, columns }}
    {#if rows.length === 0}
        <div class="text-center text-sm text-gray-500">No data available</div>
    {:else}
        {@const hasRowPreviewLimit = rowPreviewLimit > 0 && rows.length > rowPreviewLimit}
        {@const visibleRows =
            hasRowPreviewLimit && !showAllRows ? rows.slice(0, rowPreviewLimit) : rows}
        {#if hasRowPreviewLimit}
            <div
                class="mb-3 flex items-center justify-between gap-3 rounded-md border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-900"
            >
                <span>
                    Showing {showAllRows ? "all" : `the first ${rowPreviewLimit}`} of {rows.length} rows.
                </span>
                <button
                    type="button"
                    class="rounded-md border border-amber-300 bg-white px-2.5 py-1 text-sm font-medium hover:bg-amber-100"
                    onclick={() => {
                        showAllRows = !showAllRows;
                    }}
                >
                    {showAllRows ? "Show fewer rows" : "Show all"}
                </button>
            </div>
        {/if}
        <div class="max-h-[60vh] overflow-auto rounded-md border border-gray-200">
            <SvelteTable {columns} rows={visibleRows}></SvelteTable>
        </div>
    {/if}
{:catch error}
    <div class="error p-2">Error: {error.message}</div>
{/await}

<style>
    .error {
        color: #dc2626;
        background-color: #fee2e2;
        border-radius: 0.375rem;
    }
</style>
