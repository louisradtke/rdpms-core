<script lang="ts">
    import TimeSeriesTopicTreeNode from "$lib/components/datasets/TimeSeriesTopicTreeNode.svelte";
    import type { DataSetSummaryDTO } from "$lib/api_client";
    import {
        buildTopicTree,
        calculateTopicStats,
        formatCount,
        formatDateTime,
        formatDuration,
        parseTimeSeriesContainer,
        type TimeSeriesTopic
    } from "$lib/components/datasets/time-series-metadata";

    let {
        dataset,
        metadata,
        validated = false
    } = $props<{
        dataset: DataSetSummaryDTO;
        metadata: unknown;
        validated?: boolean | null;
    }>();

    let viewMode = $state<"tree" | "types">("tree");

    const container = $derived(parseTimeSeriesContainer(metadata));
    const topics = $derived(container?.topics ?? []);
    const tree = $derived(buildTopicTree(topics));
    const stats = $derived(calculateTopicStats(topics));
    const datasetBegin = $derived(
        dataset.beginStampUTC ? new Date(dataset.beginStampUTC) : stats.begin
    );
    const datasetEnd = $derived(dataset.endStampUTC ? new Date(dataset.endStampUTC) : stats.end);
    const datasetDuration = $derived(
        datasetBegin && datasetEnd
            ? Math.max(0, (datasetEnd.getTime() - datasetBegin.getTime()) / 1000)
            : stats.durationSeconds
    );
    const typeGroups = $derived.by(() => {
        const groups: [string, TimeSeriesTopic[]][] = [];
        for (const topic of topics) {
            const key = topic.messageType?.name ?? "unknown";
            const existing = groups.find(([typeName]) => typeName === key);
            if (existing) {
                existing[1] = [...existing[1], topic];
            } else {
                groups.push([key, [topic]]);
            }
        }
        return groups.sort(([left], [right]) => left.localeCompare(right));
    });

    const setViewMode = (mode: "tree" | "types") => {
        viewMode = mode;
    };
</script>

{#if container}
    <section class="space-y-3">
        <details class="rounded-lg border border-gray-200 bg-gray-50">
            <summary class="cursor-pointer list-none px-4 py-3">
                <div class="flex flex-wrap items-center gap-2">
                    <h2 class="mr-auto text-lg font-semibold text-gray-800">Time Series</h2>
                    <span
                        class={[
                            "rounded-full px-2 py-0.5 text-xs",
                            validated
                                ? "bg-green-100 text-green-800"
                                : "bg-yellow-100 text-yellow-800"
                        ]}
                    >
                        {validated ? "valid metadata" : "metadata set"}
                    </span>
                    <span class="rounded-full bg-white px-2 py-0.5 text-xs text-gray-700">
                        {stats.topicCount} topics
                    </span>
                    {#if stats.totalMessageCount !== null}
                        <span class="rounded-full bg-white px-2 py-0.5 text-xs text-gray-700">
                            {formatCount(stats.totalMessageCount)} messages
                        </span>
                    {/if}
                    <span class="rounded-full bg-white px-2 py-0.5 text-xs text-gray-700">
                        {formatDuration(datasetDuration)}
                    </span>
                </div>
            </summary>

            <div class="space-y-4 border-t border-gray-200 p-4">
                <div class="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                    <div class="rounded-md bg-white p-3">
                        <p class="text-xs font-semibold uppercase text-gray-500">Topics</p>
                        <p class="mt-1 text-xl font-semibold text-gray-900">{stats.topicCount}</p>
                        <p class="text-xs text-gray-500">{stats.typeCount} message types</p>
                    </div>
                    <div class="rounded-md bg-white p-3">
                        <p class="text-xs font-semibold uppercase text-gray-500">Messages</p>
                        <p class="mt-1 text-xl font-semibold text-gray-900">
                            {formatCount(stats.totalMessageCount)}
                        </p>
                        <p class="text-xs text-gray-500">
                            complete only if all topics report counts
                        </p>
                    </div>
                    <div class="rounded-md bg-white p-3">
                        <p class="text-xs font-semibold uppercase text-gray-500">Begin</p>
                        <p class="mt-1 text-sm text-gray-900">{formatDateTime(datasetBegin)}</p>
                    </div>
                    <div class="rounded-md bg-white p-3">
                        <p class="text-xs font-semibold uppercase text-gray-500">End</p>
                        <p class="mt-1 text-sm text-gray-900">{formatDateTime(datasetEnd)}</p>
                    </div>
                </div>

                <div class="flex flex-wrap items-center justify-between gap-3">
                    <div
                        class="inline-flex overflow-hidden rounded-md border border-gray-300 bg-white"
                    >
                        <button
                            type="button"
                            class="cursor-pointer px-3 py-1.5 text-sm"
                            class:bg-blue-600={viewMode === "tree"}
                            class:text-white={viewMode === "tree"}
                            class:bg-white={viewMode !== "tree"}
                            class:text-gray-700={viewMode !== "tree"}
                            onclick={() => setViewMode("tree")}
                        >
                            Tree
                        </button>
                        <button
                            type="button"
                            class="cursor-pointer border-l border-gray-300 px-3 py-1.5 text-sm"
                            class:bg-blue-600={viewMode === "types"}
                            class:text-white={viewMode === "types"}
                            class:bg-white={viewMode !== "types"}
                            class:text-gray-700={viewMode !== "types"}
                            onclick={() => setViewMode("types")}
                        >
                            Types
                        </button>
                    </div>
                    <p class="text-xs text-gray-500">
                        Topic details are collapsed; expand a topic for timing and type metadata.
                    </p>
                </div>

                {#if topics.length === 0}
                    <p
                        class="rounded-md border border-gray-200 bg-white px-3 py-2 text-sm text-gray-600"
                    >
                        No topics in time-series metadata.
                    </p>
                {:else if viewMode === "tree"}
                    <div class="space-y-2 rounded-lg border border-gray-200 bg-gray-100 p-3">
                        {#each tree as node (node.key)}
                            <TimeSeriesTopicTreeNode {node} />
                        {/each}
                    </div>
                {:else}
                    <div class="space-y-2 rounded-lg border border-gray-200 bg-gray-100 p-3">
                        {#each typeGroups as [typeName, group] (typeName)}
                            <details class="rounded-md border border-gray-200 bg-white">
                                <summary class="cursor-pointer list-none px-3 py-2">
                                    <div class="flex min-w-0 items-center gap-2">
                                        <span
                                            class="min-w-0 flex-1 truncate font-mono text-sm text-gray-800"
                                        >
                                            {typeName}
                                        </span>
                                        <span
                                            class="rounded bg-gray-100 px-1.5 py-0.5 text-xs text-gray-600"
                                        >
                                            {group?.length ?? 0}
                                        </span>
                                    </div>
                                </summary>
                                <div class="space-y-1 border-t border-gray-100 p-2">
                                    {#each group ?? [] as topic, index (`${topic.name ?? "unnamed"}-${index}`)}
                                        <p
                                            class="rounded bg-gray-50 px-2 py-1 font-mono text-sm text-gray-700"
                                        >
                                            {topic.name ?? "unnamed"}
                                        </p>
                                    {/each}
                                </div>
                            </details>
                        {/each}
                    </div>
                {/if}
            </div>
        </details>
    </section>
{:else}
    <section class="space-y-3">
        <details class="rounded-lg border border-yellow-200 bg-yellow-50">
            <summary class="cursor-pointer list-none px-4 py-3">
                <div class="flex flex-wrap items-center gap-2">
                    <h2 class="mr-auto text-lg font-semibold text-gray-800">Time Series</h2>
                    <span class="rounded-full bg-yellow-100 px-2 py-0.5 text-xs text-yellow-800">
                        metadata unreadable
                    </span>
                </div>
            </summary>
            <div class="border-t border-yellow-200 p-4 text-sm text-yellow-900">
                The assigned rdpms.tsdata document does not match the expected time-series container
                shape.
            </div>
        </details>
    </section>
{/if}
