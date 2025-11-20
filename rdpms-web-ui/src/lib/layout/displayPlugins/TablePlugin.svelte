<script lang="ts">
    import SvelteTable from "svelte-table";
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";

    export let dataUri: string;

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

    const MAX_SIZE = 10 * 1024 * 1024; // 10 MB

    async function fetchAndParseCSV(): Promise<TableData> {
        const response = await fetch(dataUri);

        if (!response.ok) {
            throw new Error(`Failed to fetch data: ${response.statusText}`);
        }

        // Check content length
        const contentLength = response.headers.get("content-length");
        if (contentLength && parseInt(contentLength) > MAX_SIZE) {
            throw new Error(`File size exceeds 10 MB limit (${(parseInt(contentLength) / 1024 / 1024).toFixed(2)} MB)`);
        }

        const text = await response.text();

        // Additional size check after download
        const textSize = new Blob([text]).size;
        if (textSize > MAX_SIZE) {
            throw new Error(`File size exceeds 10 MB limit (${(textSize / 1024 / 1024).toFixed(2)} MB)`);
        }

        // Parse CSV
        const lines = text.trim().split('\n');
        if (lines.length === 0) {
            throw new Error("CSV file is empty");
        }

        // Parse header row
        const headers = lines[0].split(',').map(h => h.trim().replace(/^"|"$/g, ''));

        // Create columns
        const columns: Column[] = headers.map(header => ({
            key: header,
            title: header,
            value: (v: any) => v[header] || '',
            sortable: true
        }));

        // Parse data rows
        const rows: Row[] = lines.slice(1)
            .filter(line => line.trim())
            .map(line => {
                const values = line.split(',').map(v => v.trim().replace(/^"|"$/g, ''));
                const row: Row = {};
                headers.forEach((header, index) => {
                    const value = values[index] || '';
                    // Try to convert to number if possible
                    const numValue = parseFloat(value);
                    row[header] = isNaN(numValue) ? value : numValue;
                });
                return row;
            });

        return { rows, columns };
    }

    $: tableDataPromise = fetchAndParseCSV();
</script>

{#await tableDataPromise}
    <div class="flex justify-center items-center">
        <LoadingCircle/>
    </div>
{:then { rows, columns }}
    {#if rows.length === 0}
        <div class="text-center text-sm text-gray-500">No data available</div>
    {:else}
        <SvelteTable {columns} {rows}></SvelteTable>
    {/if}
{:catch error}
    <div class="error">Error: {error.message}</div>
{/await}

<style>
    .error {
        color: #dc2626;
        background-color: #fee2e2;
        border-radius: 0.375rem;
    }
</style>