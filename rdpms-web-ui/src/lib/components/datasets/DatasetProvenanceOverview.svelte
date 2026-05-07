<script lang="ts">
    import { getOrFetchConfig, toApiConfig } from "$lib/util/config-helper";
    import { CollectionsRepository } from "$lib/data/CollectionsRepository";
    import { DataSetsRepository } from "$lib/data/DataSetsRepository";
    import { ExecutionsRepository } from "$lib/data/ExecutionsRepository";
    import type { CollectionDetailedDTO, DataSetSummaryDTO, ExecutionSummaryDTO } from "$lib/api_client";
    import { formatDateTime } from "$lib/components/datasets/time-series-metadata";

    type ProvenanceDatasetRef = {
        dataset: DataSetSummaryDTO;
        collection?: CollectionDetailedDTO;
        isCurrent?: boolean;
    };

    type ProvenanceExecutionView = {
        execution: ExecutionSummaryDTO;
        sources: ProvenanceDatasetRef[];
        outputs: ProvenanceDatasetRef[];
    };

    type ProvenanceViewState = {
        producer: ProvenanceExecutionView | null;
        downstream: ProvenanceExecutionView[];
    } | null;

    let { dataset, projectId }: { dataset: DataSetSummaryDTO; projectId: string } = $props();

    const normalize = (value?: string | null) => (value ?? "").trim();

    const datasetRouteValue = (dataset: DataSetSummaryDTO): string =>
        normalize(dataset.slug) || normalize(dataset.id);

    const collectionRouteValue = (collection?: CollectionDetailedDTO | null): string => {
        if (!collection) return "";
        return normalize(collection.slug) || normalize(collection.id);
    };

    const datasetTitle = (dataset?: DataSetSummaryDTO | null): string =>
        normalize(dataset?.name) || normalize(dataset?.slug) || normalize(dataset?.id) || "Dataset";

    const collectionTitle = (collection?: CollectionDetailedDTO | null): string =>
        normalize(collection?.name) || normalize(collection?.slug) || normalize(collection?.id) || "Collection";

    const executionTitle = (execution: ExecutionSummaryDTO): string =>
        normalize(execution.name) || normalize(execution.pipelineKey) || "Execution";

    const formatExecutionStamp = (execution: ExecutionSummaryDTO): string => {
        return formatDateTime(execution.terminatedStampUtc ?? execution.startedStampUtc ?? execution.createdStampUtc ?? null);
    };

    const buildDatasetLink = (dataset: DataSetSummaryDTO, collection?: CollectionDetailedDTO | null) => {
        const collectionId = collectionRouteValue(collection) || normalize(dataset.collectionId);
        const datasetId = datasetRouteValue(dataset);
        if (!collectionId || !datasetId) return "#";
        return `/projects/${projectId}/c/${collectionId}/${datasetId}`;
    };

    const buildCollectionLink = (collection?: CollectionDetailedDTO | null) => {
        const value = collectionRouteValue(collection);
        if (!value) return "#";
        return `/projects/${projectId}/c/${value}`;
    };

    const provenanceReq = $derived.by(async (): Promise<ProvenanceViewState> => {
        const currentDatasetId = normalize(dataset.id);
        if (!currentDatasetId) {
            return null;
        }

        const configPromise = getOrFetchConfig().then(toApiConfig);
        const executionsRepo = new ExecutionsRepository(configPromise);
        const datasetsRepo = new DataSetsRepository(configPromise);
        const collectionsRepo = new CollectionsRepository(configPromise);

        const [producerExecutions, downstreamExecutions] = await Promise.all([
            executionsRepo.listByAncestor(currentDatasetId),
            executionsRepo.listByChild(currentDatasetId)
        ]);

        const datasetIds: string[] = [currentDatasetId];
        const collectionIds: string[] = [];

        const addUnique = (list: string[], value?: string | null) => {
            const normalized = normalize(value);
            if (!normalized || list.includes(normalized)) {
                return;
            }
            list.push(normalized);
        };

        for (const execution of [...producerExecutions, ...downstreamExecutions]) {
            for (const id of execution.sourceDatasetIds ?? []) {
                addUnique(datasetIds, id);
            }
            for (const id of execution.outputDatasetIds ?? []) {
                addUnique(datasetIds, id);
            }
        }

        const datasetEntries = await Promise.all(
            datasetIds.map(async (id) => {
                const item = await datasetsRepo.getById(id);
                if (item.collectionId) {
                    addUnique(collectionIds, item.collectionId);
                }
                return [id, item] as const;
            })
        );

        const collectionEntries = await Promise.all(
            collectionIds.map(async (id) => {
                const item = await collectionsRepo.getCollectionById(id);
                return [id, item] as const;
            })
        );

        const datasetMap = new Map(datasetEntries);
        const collectionMap = new Map(collectionEntries);

        const toDatasetRef = (id: string): ProvenanceDatasetRef | null => {
            const item = datasetMap.get(id);
            if (!item) return null;
            return {
                dataset: item,
                collection: item.collectionId ? collectionMap.get(item.collectionId) : undefined,
                isCurrent: id === currentDatasetId
            };
        };

        const toExecutionView = (execution: ExecutionSummaryDTO): ProvenanceExecutionView => ({
            execution,
            sources: (execution.sourceDatasetIds ?? []).map(toDatasetRef).filter(Boolean) as ProvenanceDatasetRef[],
            outputs: (execution.outputDatasetIds ?? []).map(toDatasetRef).filter(Boolean) as ProvenanceDatasetRef[]
        });

        return {
            producer: producerExecutions[0] ? toExecutionView(producerExecutions[0]) : null,
            downstream: downstreamExecutions.map(toExecutionView)
        };
    });
</script>

{#await provenanceReq}
    <section class="space-y-3">
        <details class="rounded-lg border border-gray-200 bg-gray-50">
            <summary class="cursor-pointer list-none px-4 py-3">
                <div class="flex flex-wrap items-center gap-2">
                    <h2 class="mr-auto text-lg font-semibold text-gray-800">Provenance</h2>
                    <span class="rounded-full bg-white px-2 py-0.5 text-xs text-gray-700">
                        loading
                    </span>
                </div>
            </summary>
            <div class="border-t border-gray-200 p-4 text-sm text-gray-600">
                Loading provenance...
            </div>
        </details>
    </section>
{:then provenance}
    <section class="space-y-3">
        <details class="rounded-lg border border-gray-200 bg-gray-50">
            <summary class="cursor-pointer list-none px-4 py-3">
                <div class="flex flex-wrap items-center gap-2">
                    <h2 class="mr-auto text-lg font-semibold text-gray-800">Provenance</h2>
                    <span class="rounded-full bg-white px-2 py-0.5 text-xs text-gray-700">
                        {provenance?.producer ? "1 producer" : "no producer"}
                    </span>
                    <span class="rounded-full bg-white px-2 py-0.5 text-xs text-gray-700">
                        {provenance?.downstream.length ?? 0} downstream
                    </span>
                </div>
            </summary>

            <div class="space-y-4 border-t border-gray-200 p-4">
                {#if provenance?.producer}
                    <section class="space-y-2">
                        <h3 class="text-sm font-semibold uppercase tracking-wide text-gray-500">
                            Produced by
                        </h3>
                        <div class="rounded-md border border-gray-200 bg-white p-3">
                            <div class="flex flex-wrap items-center gap-2">
                                <span class="font-medium text-gray-800">
                                    {executionTitle(provenance.producer.execution)}
                                </span>
                                <span class="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">
                                    {formatExecutionStamp(provenance.producer.execution)}
                                </span>
                                {#if provenance.producer.execution.pipelineKey}
                                    <span class="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">
                                        pipeline {provenance.producer.execution.pipelineKey}
                                    </span>
                                {/if}
                                {#if provenance.producer.execution.trackerId}
                                    <span class="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">
                                        tracker {provenance.producer.execution.trackerId}
                                    </span>
                                {/if}
                            </div>

                            <div class="mt-3 space-y-3">
                                <div>
                                    <p class="text-xs font-semibold uppercase text-gray-500">
                                        Source datasets
                                    </p>
                                    {#if provenance.producer.sources.length === 0}
                                        <p class="text-sm text-gray-600">No source datasets registered.</p>
                                    {:else}
                                        <ul class="mt-2 space-y-2">
                                            {#each provenance.producer.sources as source (source.dataset.id)}
                                                <li class="rounded-md border border-gray-100 bg-gray-50 px-3 py-2 text-sm">
                                                    <div class="flex flex-wrap items-center gap-2">
                                                        <a
                                                            class="font-medium text-blue-700 hover:underline"
                                                            href={buildDatasetLink(source.dataset, source.collection)}
                                                        >
                                                            {datasetTitle(source.dataset)}
                                                        </a>
                                                        {#if source.isCurrent}
                                                            <span class="rounded-full bg-blue-100 px-2 py-0.5 text-xs text-blue-800">
                                                                current
                                                            </span>
                                                        {/if}
                                                        <a
                                                            class="text-xs text-gray-500 hover:underline"
                                                            href={buildCollectionLink(source.collection)}
                                                        >
                                                            collection: {collectionTitle(source.collection)}
                                                        </a>
                                                    </div>
                                                </li>
                                            {/each}
                                        </ul>
                                    {/if}
                                </div>

                                <div>
                                    <p class="text-xs font-semibold uppercase text-gray-500">
                                        Output datasets
                                    </p>
                                    {#if provenance.producer.outputs.length === 0}
                                        <p class="text-sm text-gray-600">No outputs registered.</p>
                                    {:else}
                                        <ul class="mt-2 space-y-2">
                                            {#each provenance.producer.outputs as output (output.dataset.id)}
                                                <li class="rounded-md border border-gray-100 bg-gray-50 px-3 py-2 text-sm">
                                                    <div class="flex flex-wrap items-center gap-2">
                                                        <a
                                                            class="font-medium text-blue-700 hover:underline"
                                                            href={buildDatasetLink(output.dataset, output.collection)}
                                                        >
                                                            {datasetTitle(output.dataset)}
                                                        </a>
                                                        {#if output.isCurrent}
                                                            <span class="rounded-full bg-blue-100 px-2 py-0.5 text-xs text-blue-800">
                                                                current
                                                            </span>
                                                        {/if}
                                                        <a
                                                            class="text-xs text-gray-500 hover:underline"
                                                            href={buildCollectionLink(output.collection)}
                                                        >
                                                            collection: {collectionTitle(output.collection)}
                                                        </a>
                                                    </div>
                                                </li>
                                            {/each}
                                        </ul>
                                    {/if}
                                </div>
                            </div>
                        </div>
                    </section>
                {/if}

                <section class="space-y-2">
                    <h3 class="text-sm font-semibold uppercase tracking-wide text-gray-500">
                        Used by
                    </h3>
                    {#if (provenance?.downstream.length ?? 0) === 0}
                        <p class="rounded-md border border-gray-200 bg-white px-3 py-2 text-sm text-gray-600">
                            No downstream executions registered.
                        </p>
                    {:else}
                        <div class="space-y-3">
                            {#each provenance?.downstream ?? [] as executionView (executionView.execution.id)}
                                <div class="rounded-md border border-gray-200 bg-white p-3">
                                    <div class="flex flex-wrap items-center gap-2">
                                        <span class="font-medium text-gray-800">
                                            {executionTitle(executionView.execution)}
                                        </span>
                                        <span class="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">
                                            {formatExecutionStamp(executionView.execution)}
                                        </span>
                                        {#if executionView.execution.pipelineKey}
                                            <span class="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">
                                                pipeline {executionView.execution.pipelineKey}
                                            </span>
                                        {/if}
                                        {#if executionView.execution.trackerId}
                                            <span class="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">
                                                tracker {executionView.execution.trackerId}
                                            </span>
                                        {/if}
                                    </div>

                                    <div class="mt-3 space-y-3">
                                        <div>
                                            <p class="text-xs font-semibold uppercase text-gray-500">
                                                Source datasets
                                            </p>
                                            {#if executionView.sources.length === 0}
                                                <p class="text-sm text-gray-600">No source datasets registered.</p>
                                            {:else}
                                                <ul class="mt-2 space-y-2">
                                                    {#each executionView.sources as source (source.dataset.id)}
                                                        <li class="rounded-md border border-gray-100 bg-gray-50 px-3 py-2 text-sm">
                                                            <div class="flex flex-wrap items-center gap-2">
                                                                <a
                                                                    class="font-medium text-blue-700 hover:underline"
                                                                    href={buildDatasetLink(source.dataset, source.collection)}
                                                                >
                                                                    {datasetTitle(source.dataset)}
                                                                </a>
                                                                {#if source.isCurrent}
                                                                    <span class="rounded-full bg-blue-100 px-2 py-0.5 text-xs text-blue-800">
                                                                        current
                                                                    </span>
                                                                {/if}
                                                                <a
                                                                    class="text-xs text-gray-500 hover:underline"
                                                                    href={buildCollectionLink(source.collection)}
                                                                >
                                                                    collection: {collectionTitle(source.collection)}
                                                                </a>
                                                            </div>
                                                        </li>
                                                    {/each}
                                                </ul>
                                            {/if}
                                        </div>

                                        <div>
                                            <p class="text-xs font-semibold uppercase text-gray-500">
                                                Output datasets
                                            </p>
                                            {#if executionView.outputs.length === 0}
                                                <p class="text-sm text-gray-600">No outputs registered.</p>
                                            {:else}
                                                <ul class="mt-2 space-y-2">
                                                    {#each executionView.outputs as output (output.dataset.id)}
                                                        <li class="rounded-md border border-gray-100 bg-gray-50 px-3 py-2 text-sm">
                                                            <div class="flex flex-wrap items-center gap-2">
                                                                <a
                                                                    class="font-medium text-blue-700 hover:underline"
                                                                    href={buildDatasetLink(output.dataset, output.collection)}
                                                                >
                                                                    {datasetTitle(output.dataset)}
                                                                </a>
                                                                {#if output.isCurrent}
                                                                    <span class="rounded-full bg-blue-100 px-2 py-0.5 text-xs text-blue-800">
                                                                        current
                                                                    </span>
                                                                {/if}
                                                                <a
                                                                    class="text-xs text-gray-500 hover:underline"
                                                                    href={buildCollectionLink(output.collection)}
                                                                >
                                                                    collection: {collectionTitle(output.collection)}
                                                                </a>
                                                            </div>
                                                        </li>
                                                    {/each}
                                                </ul>
                                            {/if}
                                        </div>

                                        {#if (executionView.execution.metadataIds?.length ?? 0) > 0}
                                            <div>
                                                <p class="text-xs font-semibold uppercase text-gray-500">
                                                    Metadata written
                                                </p>
                                                <div class="mt-2 flex flex-wrap gap-2">
                                                    {#each executionView.execution.metadataIds ?? [] as metadataId (metadataId)}
                                                        <span class="rounded-full bg-gray-100 px-2 py-0.5 font-mono text-xs text-gray-700">
                                                            {metadataId}
                                                        </span>
                                                    {/each}
                                                </div>
                                            </div>
                                        {/if}
                                    </div>
                                </div>
                            {/each}
                        </div>
                    {/if}
                </section>
            </div>
        </details>
    </section>
{:catch error}
    <section class="space-y-3">
        <details class="rounded-lg border border-yellow-200 bg-yellow-50">
            <summary class="cursor-pointer list-none px-4 py-3">
                <div class="flex flex-wrap items-center gap-2">
                    <h2 class="mr-auto text-lg font-semibold text-gray-800">Provenance</h2>
                    <span class="rounded-full bg-yellow-100 px-2 py-0.5 text-xs text-yellow-800">
                        unavailable
                    </span>
                </div>
            </summary>
            <div class="border-t border-yellow-200 p-4 text-sm text-yellow-900">
                Failed to load provenance: {error.message}
            </div>
        </details>
    </section>
{/await}
