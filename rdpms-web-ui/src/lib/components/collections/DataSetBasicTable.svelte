<script lang="ts">
    import type { DataSetSummaryDTO } from "$lib/api_client";
    import DatasetExpandedDetails from "$lib/components/collections/DatasetExpandedDetails.svelte";
    import { DataSetsRepository } from "$lib/data/DataSetsRepository";
    import CodeCopyField from "$lib/layout/CodeCopyField.svelte";
    import {
        getDatasetDurationMs,
        getDatasetFileCount,
        getDatasetTotalSize,
        preserveDatasetListParams,
        type DatasetListQuery,
        type DatasetSortKey
    } from "$lib/util/dataset-list-query";
    import { getOrFetchConfig, toApiConfig } from "$lib/util/config-helper";

    let {
        datasets,
        projectSlug,
        collectionSlug,
        query,
        searchParams,
        totalCount,
        onQueryChange,
        onDelete
    } = $props<{
        datasets: DataSetSummaryDTO[];
        projectSlug: string;
        collectionSlug: string;
        query: DatasetListQuery;
        searchParams: URLSearchParams;
        totalCount: number;
        onQueryChange: (query: DatasetListQuery) => void;
        onDelete: () => void;
    }>();

    const dayFormatter = new Intl.DateTimeFormat(undefined, {
        year: "numeric",
        month: "short",
        day: "numeric",
        timeZone: "UTC"
    });

    const timeFormatter = new Intl.DateTimeFormat(undefined, {
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
        timeZone: "UTC"
    });

    const dateTimeFormatter = new Intl.DateTimeFormat(undefined, {
        year: "numeric",
        month: "short",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
        timeZone: "UTC",
        timeZoneName: "short"
    });

    const formatDateTime = (value?: Date | null) => (value ? dateTimeFormatter.format(value) : "—");
    const formatDate = (value?: Date | null) => (value ? dayFormatter.format(value) : "—");
    const formatTime = (value?: Date | null) => (value ? timeFormatter.format(value) : "—");

    const formatDuration = (durationMs: number | null) => {
        if (durationMs === null) return "—";

        const totalSeconds = Math.round(durationMs / 1000);
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;

        if (hours > 0) return `${hours}h ${minutes}m ${seconds}s`;
        if (minutes > 0) return `${minutes}m ${seconds}s`;
        return `${seconds}s`;
    };

    const formatBytes = (value: number | null) => {
        if (value === null || !Number.isFinite(value)) return "—";
        if (value === 0) return "0 B";

        const units = ["B", "KB", "MB", "GB", "TB"];
        const exponent = Math.min(Math.floor(Math.log(value) / Math.log(1024)), units.length - 1);
        const scaled = value / Math.pow(1024, exponent);
        const precision = scaled >= 10 || exponent === 0 ? 0 : 1;

        return `${scaled.toFixed(precision)} ${units[exponent]}`;
    };

    const datasetName = (dataset: DataSetSummaryDTO) =>
        dataset.name ?? dataset.slug ?? dataset.id ?? "Untitled dataset";

    const datasetRouteValue = (dataset: DataSetSummaryDTO) => dataset.slug ?? dataset.id ?? "";

    const datasetHref = (dataset: DataSetSummaryDTO) => {
        const routeValue = datasetRouteValue(dataset);
        if (!routeValue) return "";

        const url = new URL(
            `/projects/${projectSlug}/c/${collectionSlug}/${routeValue}`,
            window.location.origin
        );
        preserveDatasetListParams(url, searchParams);
        return `${url.pathname}${url.search}`;
    };

    let expandedIds = $state<string[]>([]);
    let deletingId = $state<string | null>(null);
    let deleteError = $state("");

    const isExpanded = (dataset: DataSetSummaryDTO) =>
        Boolean(dataset.id && expandedIds.includes(dataset.id));

    const toggleExpanded = (dataset: DataSetSummaryDTO) => {
        if (!dataset.id) return;

        expandedIds = isExpanded(dataset)
            ? expandedIds.filter((id) => id !== dataset.id)
            : [...expandedIds, dataset.id];
    };

    const updateSort = (sort: DatasetSortKey) => {
        onQueryChange({
            ...query,
            sort,
            dir: query.sort === sort && query.dir === "asc" ? "desc" : "asc"
        });
    };

    const sortIndicator = (sort: DatasetSortKey) => {
        if (query.sort !== sort) return "";
        return query.dir === "asc" ? " ↑" : " ↓";
    };

    async function deleteDataset(dataset: DataSetSummaryDTO): Promise<void> {
        if (!dataset.id) return;
        const confirmed = window.confirm(`Delete dataset "${dataset.name ?? dataset.id}"?`);
        if (!confirmed) return;

        deletingId = dataset.id;
        deleteError = "";

        try {
            const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));
            await repo.deleteById(dataset.id);
            onDelete();
        } catch (e) {
            deleteError = e instanceof Error ? e.message : "Failed to delete dataset.";
        } finally {
            deletingId = null;
        }
    }
</script>

<div class="space-y-3">
    {#if deleteError}
        <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
            {deleteError}
        </p>
    {/if}

    <div class="text-sm text-gray-500">
        Showing {datasets.length} of {totalCount} datasets
    </div>

    {#if totalCount === 0}
        <p class="rounded-md border border-gray-200 bg-white px-4 py-5 text-sm text-gray-600">
            No datasets in this collection.
        </p>
    {:else if datasets.length === 0}
        <p class="rounded-md border border-gray-200 bg-white px-4 py-5 text-sm text-gray-600">
            No datasets match the active filters.
        </p>
    {:else}
        <div class="max-w-full overflow-x-auto rounded-md border border-gray-200 bg-white">
            <table class="w-full min-w-[820px] table-fixed text-sm">
                <colgroup>
                    <col class="w-[43%]" />
                    <col class="w-[14%]" />
                    <col class="w-[11%]" />
                    <col class="w-[9%]" />
                    <col class="w-[12%]" />
                    <col class="w-[11%]" />
                </colgroup>
                <thead class="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
                    <tr>
                        <th class="px-4 py-2 text-left font-medium">
                            <button
                                type="button"
                                class="hover:text-gray-800"
                                onclick={() => updateSort("name")}
                            >
                                Dataset{sortIndicator("name")}
                            </button>
                        </th>
                        <th class="px-3 py-2 text-left font-medium">
                            <button
                                type="button"
                                class="hover:text-gray-800"
                                onclick={() => updateSort("begin")}
                            >
                                Begin{sortIndicator("begin")}
                            </button>
                        </th>
                        <th class="px-3 py-2 text-right font-medium">
                            <button
                                type="button"
                                class="hover:text-gray-800"
                                onclick={() => updateSort("duration")}
                            >
                                Duration{sortIndicator("duration")}
                            </button>
                        </th>
                        <th class="px-3 py-2 text-right font-medium">
                            <button
                                type="button"
                                class="hover:text-gray-800"
                                onclick={() => updateSort("fileCount")}
                            >
                                Count{sortIndicator("fileCount")}
                            </button>
                        </th>
                        <th class="px-3 py-2 text-right font-medium">
                            <button
                                type="button"
                                class="hover:text-gray-800"
                                onclick={() => updateSort("totalSizeBytes")}
                            >
                                Sum{sortIndicator("totalSizeBytes")}
                            </button>
                        </th>
                        <th class="px-4 py-2 text-right font-medium">Actions</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-100">
                    {#each datasets as dataset, index (dataset.id ?? dataset.slug ?? index)}
                        {@const href = datasetHref(dataset)}
                        {@const expanded = isExpanded(dataset)}
                        <tr class="hover:bg-gray-50">
                            <td class="px-4 py-3 align-top">
                                <div class="min-w-0">
                                    {#if href}
                                        <a
                                            {href}
                                            class="break-words font-medium text-blue-600 [overflow-wrap:anywhere] hover:underline"
                                            title={datasetName(dataset)}
                                        >
                                            {datasetName(dataset)}
                                        </a>
                                    {:else}
                                        <span
                                            class="break-words font-medium text-gray-900 [overflow-wrap:anywhere]"
                                        >
                                            {datasetName(dataset)}
                                        </span>
                                    {/if}

                                    {#if dataset.id}
                                        <div
                                            class="mt-1 flex min-w-0 items-center gap-1 text-xs text-gray-500"
                                        >
                                            <span class="shrink-0">ID</span>
                                            <span
                                                class="min-w-0 truncate font-mono"
                                                title={dataset.id}>{dataset.id}</span
                                            >
                                            <CodeCopyField
                                                text={dataset.id}
                                                copyOnly={true}
                                                tooltip="Copy dataset ID"
                                            />
                                        </div>
                                    {/if}
                                </div>
                            </td>
                            <td class="whitespace-nowrap px-3 py-3 align-top text-xs text-gray-700">
                                <div>{formatDate(dataset.beginStampUTC)}</div>
                                <div class="font-mono">{formatTime(dataset.beginStampUTC)}</div>
                            </td>
                            <td
                                class="whitespace-nowrap px-3 py-3 text-right align-top tabular-nums text-gray-700"
                            >
                                {formatDuration(getDatasetDurationMs(dataset))}
                            </td>
                            <td
                                class="whitespace-nowrap px-3 py-3 text-right align-top tabular-nums text-gray-700"
                            >
                                {getDatasetFileCount(dataset)}
                            </td>
                            <td
                                class="whitespace-nowrap px-3 py-3 text-right align-top tabular-nums text-gray-700"
                            >
                                {formatBytes(getDatasetTotalSize(dataset))}
                            </td>
                            <td class="px-4 py-3 text-right align-top">
                                <div class="flex justify-end gap-1">
                                    <button
                                        type="button"
                                        class="inline-flex size-8 items-center justify-center rounded-md text-gray-600 hover:bg-gray-100 hover:text-gray-800 disabled:cursor-default disabled:opacity-50"
                                        aria-label={`${expanded ? "Collapse" : "Expand"} dataset ${datasetName(dataset)}`}
                                        aria-expanded={expanded}
                                        title={expanded ? "Collapse details" : "Expand details"}
                                        disabled={!dataset.id}
                                        onclick={() => toggleExpanded(dataset)}
                                    >
                                        <span aria-hidden="true">{expanded ? "−" : "+"}</span>
                                    </button>
                                    <button
                                        type="button"
                                        class="inline-flex size-8 items-center justify-center rounded-md text-red-600 hover:bg-red-50 hover:text-red-700 disabled:cursor-default disabled:opacity-50"
                                        aria-label={`Delete dataset ${datasetName(dataset)}`}
                                        title={deletingId === dataset.id
                                            ? "Deleting dataset"
                                            : "Delete dataset"}
                                        disabled={deletingId === dataset.id}
                                        onclick={() => deleteDataset(dataset)}
                                    >
                                        {#if deletingId === dataset.id}
                                            <span aria-hidden="true" class="text-xs font-semibold"
                                                >...</span
                                            >
                                        {:else}
                                            <svg
                                                aria-hidden="true"
                                                viewBox="0 0 20 20"
                                                fill="currentColor"
                                                class="size-4"
                                            >
                                                <path
                                                    fill-rule="evenodd"
                                                    d="M8.5 2a1.5 1.5 0 0 0-1.415 1H4.75a.75.75 0 0 0 0 1.5h10.5a.75.75 0 0 0 0-1.5h-2.335A1.5 1.5 0 0 0 11.5 2h-3Zm-2.8 4a.75.75 0 0 0-.747.812l.7 8.4A2 2 0 0 0 7.647 17h4.706a2 2 0 0 0 1.993-1.788l.7-8.4A.75.75 0 0 0 14.3 6H5.7Zm2.55 2.25a.75.75 0 0 1 .75.75v5a.75.75 0 0 1-1.5 0V9a.75.75 0 0 1 .75-.75Zm3.5 0a.75.75 0 0 1 .75.75v5a.75.75 0 0 1-1.5 0V9a.75.75 0 0 1 .75-.75Z"
                                                    clip-rule="evenodd"
                                                />
                                            </svg>
                                        {/if}
                                    </button>
                                </div>
                            </td>
                        </tr>
                        {#if expanded}
                            <tr>
                                <td colspan="6" class="p-0">
                                    <DatasetExpandedDetails
                                        {dataset}
                                        {formatDateTime}
                                        {formatDuration}
                                    />
                                </td>
                            </tr>
                        {/if}
                    {/each}
                </tbody>
            </table>
        </div>
    {/if}
</div>
