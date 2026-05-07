<script lang="ts">
    import TimeSeriesTopicTreeNode from "$lib/components/datasets/TimeSeriesTopicTreeNode.svelte";
    import {
        collectTopics,
        formatCount,
        formatDateTime,
        formatDuration,
        formatRate,
        parseDate,
        topicDurationSeconds,
        typeBasename,
        type TopicTreeNode
    } from "$lib/components/datasets/time-series-metadata";

    let { node, level = 0 } = $props<{
        node: TopicTreeNode;
        level?: number;
    }>();

    const indentClass = $derived(
        level === 0 ? "" : level === 1 ? "pl-3" : level === 2 ? "pl-5" : "pl-7"
    );

    const folderTopics = $derived(node.kind === "folder" ? collectTopics(node.children) : []);
    const folderMessageCount = $derived.by(() => {
        if (node.kind !== "folder" || folderTopics.length === 0) return null;
        let total = 0;
        for (const topic of folderTopics) {
            if (topic.metadata?.messageCount === undefined) return null;
            total += topic.metadata.messageCount;
        }
        return total;
    });
</script>

{#if node.kind === "folder"}
    <details class={["rounded-md border border-gray-200 bg-white", indentClass]}>
        <summary class="flex cursor-pointer list-none items-center gap-2 px-3 py-2 text-sm">
            <span class="text-gray-400">&gt;</span>
            <span class="min-w-0 flex-1 truncate font-medium text-gray-800">/{node.name}</span>
            <span class="shrink-0 rounded bg-gray-100 px-1.5 py-0.5 text-xs text-gray-600">
                {node.topicCount}
            </span>
            {#if folderMessageCount !== null}
                <span class="hidden shrink-0 text-xs text-gray-500 sm:inline">
                    {formatCount(folderMessageCount)} msgs
                </span>
            {/if}
        </summary>
        <div class="space-y-1 border-t border-gray-100 p-2">
            {#each node.children as child (child.key)}
                <TimeSeriesTopicTreeNode node={child} level={level + 1} />
            {/each}
        </div>
    </details>
{:else}
    {@const first = parseDate(node.topic.metadata?.firstMessageTimestamp)}
    {@const last = parseDate(node.topic.metadata?.lastMessageTimestamp)}
    {@const typeName = typeBasename(node.topic.messageType)}
    <details class={["rounded-md border border-gray-200 bg-white", indentClass]}>
        <summary class="cursor-pointer list-none px-3 py-2">
            <div class="flex min-w-0 flex-wrap items-center gap-x-3 gap-y-1 text-sm">
                <span class="min-w-0 flex-1 truncate font-mono text-gray-800" title={node.path}>
                    /{node.name}
                </span>
                <span class="shrink-0 rounded bg-blue-50 px-1.5 py-0.5 text-xs text-blue-800">
                    {typeName}
                </span>
                <span class="hidden shrink-0 text-xs text-gray-500 sm:inline">
                    {formatRate(node.topic)}
                </span>
            </div>
        </summary>

        <div class="grid gap-3 border-t border-gray-100 px-3 py-3 text-sm md:grid-cols-2">
            <div class="space-y-1">
                <p class="text-xs font-semibold uppercase text-gray-500">Topic</p>
                <p class="break-all font-mono text-gray-800">{node.path}</p>
                <p class="text-gray-600">
                    {node.topic.messageType?.name ?? "Unknown message type"}
                </p>
                {#if node.topic.messageType?.reference}
                    <a
                        href={node.topic.messageType.reference}
                        target="_blank"
                        rel="noreferrer"
                        class="inline-flex text-sm text-blue-700 hover:underline"
                    >
                        Open type reference
                    </a>
                {/if}
            </div>

            <dl class="grid grid-cols-2 gap-x-3 gap-y-2">
                <div>
                    <dt class="text-xs font-semibold uppercase text-gray-500">Messages</dt>
                    <dd class="text-gray-800">{formatCount(node.topic.metadata?.messageCount)}</dd>
                </div>
                <div>
                    <dt class="text-xs font-semibold uppercase text-gray-500">Approx. rate</dt>
                    <dd class="text-gray-800">{formatRate(node.topic)}</dd>
                </div>
                <div>
                    <dt class="text-xs font-semibold uppercase text-gray-500">Duration</dt>
                    <dd class="text-gray-800">
                        {formatDuration(topicDurationSeconds(node.topic))}
                    </dd>
                </div>
                <div>
                    <dt class="text-xs font-semibold uppercase text-gray-500">Fields</dt>
                    <dd class="text-gray-800">{node.topic.messageType?.fields?.length ?? 0}</dd>
                </div>
            </dl>

            <div class="md:col-span-2">
                <div class="grid gap-2 text-xs text-gray-600 sm:grid-cols-2">
                    <p>
                        <span class="font-semibold text-gray-500">First:</span>
                        {formatDateTime(first)}
                    </p>
                    <p>
                        <span class="font-semibold text-gray-500">Last:</span>
                        {formatDateTime(last)}
                    </p>
                </div>
            </div>
        </div>
    </details>
{/if}
