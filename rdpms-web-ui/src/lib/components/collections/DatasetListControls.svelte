<script lang="ts">
    import { DeletionStateDTO } from "$lib/api_client";
    import {
        DEFAULT_DATASET_LIST_QUERY,
        hasDatasetFilters,
        resetDatasetFilters,
        type DatasetListQuery,
        type DatasetSortKey
    } from "$lib/util/dataset-list-query";

    let {
        query,
        onChange,
        compact = false,
        collapsedByDefault = false
    } = $props<{
        query: DatasetListQuery;
        onChange: (query: DatasetListQuery) => void;
        compact?: boolean;
        collapsedByDefault?: boolean;
    }>();

    let userExpanded = $state<boolean | null>(null);
    const expanded = $derived(userExpanded ?? !collapsedByDefault);

    const sortOptions: { value: DatasetSortKey; label: string }[] = [
        { value: "begin", label: "Begin" },
        { value: "duration", label: "Duration" },
        { value: "name", label: "Name" },
        { value: "created", label: "Created" },
        { value: "fileCount", label: "File count" },
        { value: "totalSizeBytes", label: "Size" },
        { value: "deletionState", label: "Deletion state" }
    ];

    const update = (patch: Partial<DatasetListQuery>) => {
        onChange({ ...query, ...patch });
    };

    const resetSorting = () => {
        onChange({
            ...query,
            sort: DEFAULT_DATASET_LIST_QUERY.sort,
            dir: DEFAULT_DATASET_LIST_QUERY.dir
        });
    };

    const clearFilters = () => {
        onChange(resetDatasetFilters(query));
    };

    const toggleDeletionState = (state: string, checked: boolean) => {
        onChange({
            ...query,
            deletionState: checked
                ? [...query.deletionState, state]
                : query.deletionState.filter((value: string) => value !== state)
        });
    };
</script>

<div
    class={[
        "rounded-md border border-gray-200 bg-white text-sm text-gray-700",
        compact ? "space-y-3 p-3" : "space-y-4 p-4"
    ]}
>
    <button
        type="button"
        class="flex w-full items-center justify-between text-left text-sm font-medium text-gray-800"
        aria-expanded={expanded}
        onclick={() => (userExpanded = !expanded)}
    >
        <span>Dataset list controls</span>
        <span aria-hidden="true">{expanded ? "−" : "+"}</span>
    </button>

    {#if expanded}
        <div class={["flex gap-3", compact ? "flex-col" : "flex-wrap items-end"]}>
            <label class={compact ? "block" : "block min-w-40"}>
                <span class="mb-1 block text-xs font-medium uppercase text-gray-500">Sort</span>
                <select
                    class="w-full rounded-md border border-gray-300 bg-white px-2 py-1.5 text-sm"
                    value={query.sort}
                    onchange={(event) =>
                        update({ sort: event.currentTarget.value as DatasetSortKey })}
                >
                    {#each sortOptions as option (option.value)}
                        <option value={option.value}>{option.label}</option>
                    {/each}
                </select>
            </label>

            <button
                type="button"
                class="inline-flex items-center justify-center rounded-md border border-gray-300 px-3 py-1.5 text-sm hover:bg-gray-50"
                onclick={() => update({ dir: query.dir === "asc" ? "desc" : "asc" })}
                title="Toggle dataset sort direction"
            >
                {query.dir === "asc" ? "Ascending" : "Descending"}
            </button>

            <button
                type="button"
                class="inline-flex items-center justify-center rounded-md border border-gray-300 px-3 py-1.5 text-sm hover:bg-gray-50"
                onclick={resetSorting}
            >
                Reset sort
            </button>
        </div>

        <div class={compact ? "space-y-3" : "grid gap-3 md:grid-cols-2 xl:grid-cols-4"}>
            <label class="block">
                <span class="mb-1 block text-xs font-medium uppercase text-gray-500">Begin min</span
                >
                <input
                    type="datetime-local"
                    class="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm"
                    value={query.beginMin}
                    oninput={(event) => update({ beginMin: event.currentTarget.value })}
                />
            </label>

            <label class="block">
                <span class="mb-1 block text-xs font-medium uppercase text-gray-500">Begin max</span
                >
                <input
                    type="datetime-local"
                    class="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm"
                    value={query.beginMax}
                    oninput={(event) => update({ beginMax: event.currentTarget.value })}
                />
            </label>

            <label class="block">
                <span class="mb-1 block text-xs font-medium uppercase text-gray-500">End min</span>
                <input
                    type="datetime-local"
                    class="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm"
                    value={query.endMin}
                    oninput={(event) => update({ endMin: event.currentTarget.value })}
                />
            </label>

            <label class="block">
                <span class="mb-1 block text-xs font-medium uppercase text-gray-500">End max</span>
                <input
                    type="datetime-local"
                    class="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm"
                    value={query.endMax}
                    oninput={(event) => update({ endMax: event.currentTarget.value })}
                />
            </label>

            <label class="block">
                <span class="mb-1 block text-xs font-medium uppercase text-gray-500"
                    >Duration min (s)</span
                >
                <input
                    type="number"
                    min="0"
                    class="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm"
                    value={query.durationMin}
                    oninput={(event) => update({ durationMin: event.currentTarget.value })}
                />
            </label>

            <label class="block">
                <span class="mb-1 block text-xs font-medium uppercase text-gray-500"
                    >Duration max (s)</span
                >
                <input
                    type="number"
                    min="0"
                    class="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm"
                    value={query.durationMax}
                    oninput={(event) => update({ durationMax: event.currentTarget.value })}
                />
            </label>

            <label class="block">
                <span class="mb-1 block text-xs font-medium uppercase text-gray-500"
                    >File count min</span
                >
                <input
                    type="number"
                    min="0"
                    class="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm"
                    value={query.fileCountMin}
                    oninput={(event) => update({ fileCountMin: event.currentTarget.value })}
                />
            </label>

            <label class="block">
                <span class="mb-1 block text-xs font-medium uppercase text-gray-500"
                    >File count max</span
                >
                <input
                    type="number"
                    min="0"
                    class="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm"
                    value={query.fileCountMax}
                    oninput={(event) => update({ fileCountMax: event.currentTarget.value })}
                />
            </label>

            <fieldset class="block">
                <legend class="mb-1 block text-xs font-medium uppercase text-gray-500"
                    >Deletion state</legend
                >
                <div class="space-y-1 rounded-md border border-gray-300 px-2 py-1.5">
                    {#each Object.values(DeletionStateDTO) as state (state)}
                        <label class="flex items-center gap-2">
                            <input
                                type="checkbox"
                                checked={query.deletionState.includes(state)}
                                onchange={(event) =>
                                    toggleDeletionState(state, event.currentTarget.checked)}
                            />
                            <span>{state}</span>
                        </label>
                    {/each}
                </div>
            </fieldset>
        </div>

        {#if hasDatasetFilters(query)}
            <button
                type="button"
                class="inline-flex rounded-md border border-gray-300 px-3 py-1.5 text-sm hover:bg-gray-50"
                onclick={clearFilters}
            >
                Clear filters
            </button>
        {/if}
    {/if}
</div>
