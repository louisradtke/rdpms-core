<script lang="ts">
    import type { AssignedMetaDateDTO, DataSetSummaryDTO } from "$lib/api_client";
    import CodeCopyField from "$lib/layout/CodeCopyField.svelte";
    import { getDatasetDurationMs, getDatasetFileCount } from "$lib/util/dataset-list-query";

    let { dataset, formatDateTime, formatDuration } = $props<{
        dataset: DataSetSummaryDTO;
        formatDateTime: (value?: Date | null) => string;
        formatDuration: (value: number | null) => string;
    }>();

    const assignedMetaDates = $derived(dataset.metaDates ?? []);

    const metaLabel = (meta: AssignedMetaDateDTO) =>
        meta.metadataKey ?? meta.metadataId ?? "metadata";
</script>

<div class="grid gap-4 bg-gray-50 px-4 py-4 text-sm text-gray-700 md:grid-cols-[1.4fr_1fr]">
    <section>
        <h3 class="mb-2 text-xs font-semibold uppercase text-gray-500">Assigned metadata</h3>
        {#if assignedMetaDates.length === 0}
            <p class="text-gray-500">No dataset metadata assigned.</p>
        {:else}
            <div class="flex flex-wrap gap-2">
                {#each assignedMetaDates as meta (meta.metadataKey ?? meta.metadataId)}
                    <span
                        class={[
                            "inline-flex items-center gap-1 rounded-full px-2 py-1 text-xs",
                            meta.collectionSchemaVerified
                                ? "bg-green-100 text-green-800"
                                : "bg-yellow-100 text-yellow-800"
                        ]}
                        title={meta.metadataId ?? ""}
                    >
                        <span
                            class={[
                                "h-2 w-2 rounded-full",
                                meta.collectionSchemaVerified ? "bg-green-500" : "bg-yellow-500"
                            ]}
                        ></span>
                        {metaLabel(meta)}
                    </span>
                {/each}
            </div>
        {/if}
    </section>

    <dl class="grid grid-cols-[max-content_1fr] gap-x-4 gap-y-2">
        <dt class="text-xs font-semibold uppercase text-gray-500">Created</dt>
        <dd>{formatDateTime(dataset.createdStampUTC)}</dd>

        <dt class="text-xs font-semibold uppercase text-gray-500">Begin</dt>
        <dd>{formatDateTime(dataset.beginStampUTC)}</dd>

        <dt class="text-xs font-semibold uppercase text-gray-500">End</dt>
        <dd>{formatDateTime(dataset.endStampUTC)}</dd>

        <dt class="text-xs font-semibold uppercase text-gray-500">Duration</dt>
        <dd>{formatDuration(getDatasetDurationMs(dataset))}</dd>

        <dt class="text-xs font-semibold uppercase text-gray-500">Files</dt>
        <dd>{getDatasetFileCount(dataset)}</dd>

        <dt class="text-xs font-semibold uppercase text-gray-500">Lifecycle</dt>
        <dd>{dataset.lifecycleState ?? "—"}</dd>

        <dt class="text-xs font-semibold uppercase text-gray-500">Deletion</dt>
        <dd>{dataset.deletionState ?? "—"}</dd>

        {#if dataset.slug}
            <dt class="text-xs font-semibold uppercase text-gray-500">Slug</dt>
            <dd class="flex min-w-0 items-center gap-1">
                <span class="truncate font-mono text-xs" title={dataset.slug}>{dataset.slug}</span>
                <CodeCopyField text={dataset.slug} copyOnly={true} tooltip="Copy dataset slug" />
            </dd>
        {/if}
    </dl>
</div>
