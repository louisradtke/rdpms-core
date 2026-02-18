<script lang="ts">
    import type { DataSetSummaryDTO, MetaDateCollectionColumnDTO } from '$lib/api_client';
    import MetadataAssignmentModal from '$lib/components/collections/MetadataAssignmentModal.svelte';
    import type { MetadataAssignmentTarget } from '$lib/components/collections/metadata-modal-types';
    import { findAssignedMeta } from '$lib/util/meta-date-utils';

    let { datasets, columns, projectSlug, collectionSlug, onDataChanged } = $props<{
        datasets: DataSetSummaryDTO[];
        columns: MetaDateCollectionColumnDTO[];
        projectSlug: string;
        collectionSlug: string;
        onDataChanged: () => void;
    }>();

    let modalOpen = $state(false);
    let modalTarget = $state<MetadataAssignmentTarget | null>(null);

    const closeModal = () => {
        modalOpen = false;
    };

    const openMetadataModal = (
        dataset: DataSetSummaryDTO,
        column: MetaDateCollectionColumnDTO,
        metadataId?: string | null,
        validated?: boolean | null
    ) => {
        const metadataKey = column.metadataKey ?? 'unknown';
        const datasetName = dataset.name ?? dataset.id ?? 'Dataset';
        const datasetId = dataset.id ?? '';

        if (!datasetId) {
            return;
        }

        modalTarget = {
            targetType: 'dataset',
            targetId: datasetId,
            title: `${datasetName} / ${metadataKey}`,
            metadataKey,
            metadataId: metadataId ?? null,
            schemaDbId: column.schema?.id,
            schemaId: column.schema?.schemaId,
            validated: validated ?? false
        };

        modalOpen = true;
    };
</script>

<div class="mb-2 flex items-center gap-3 text-sm text-gray-600">
    <span class="inline-flex items-center gap-1">
        <span class="inline-block h-2 w-2 rounded-full bg-green-500"></span>
        Validated
    </span>
    <span class="inline-flex items-center gap-1">
        <span class="inline-block h-2 w-2 rounded-full bg-yellow-500"></span>
        Set, not validated
    </span>
    <span class="inline-flex items-center gap-1">
        <span class="inline-block h-2 w-2 rounded-full bg-gray-300"></span>
        Missing
    </span>
    <span class="ml-auto text-xs text-gray-500">
        Hint: click any status badge to open the editor.
    </span>
</div>

<div class="overflow-x-auto">
    <table class="table-auto w-full">
        <thead>
            <tr>
                <th class="text-left w-48">Dataset Name</th>
                {#each columns as column (column.metadataKey)}
                    <th class="text-center min-w-[140px]" title={column.schema?.schemaId ?? column.schema?.id ?? ''}>
                        {column.metadataKey ?? 'unknown'}
                    </th>
                {/each}
            </tr>
        </thead>
        <tbody>
            {#each datasets as dataset (dataset.id)}
                <tr>
                    <td class="text-left">
                        <a href="/projects/{projectSlug}/c/{collectionSlug}/{dataset.slug ?? dataset.id}" class="text-blue-500 hover:underline">
                            {dataset.name}
                        </a>
                    </td>
                    {#each columns as column (column.metadataKey)}
                        {@const assigned = findAssignedMeta(dataset, column)}
                        {@const hasMeta = Boolean(assigned)}
                        {@const validated = assigned?.collectionSchemaVerified}
                        <td class="text-center">
                            <button
                                type="button"
                                class="inline-flex cursor-pointer items-center gap-1 rounded-full px-2 py-0.5 text-xs hover:ring-1 hover:ring-gray-300"
                                class:bg-green-100={hasMeta && Boolean(validated)}
                                class:text-green-800={hasMeta && Boolean(validated)}
                                class:bg-yellow-100={hasMeta && !Boolean(validated)}
                                class:text-yellow-800={hasMeta && !Boolean(validated)}
                                class:bg-gray-100={!hasMeta}
                                class:text-gray-600={!hasMeta}
                                title={hasMeta ? `Open metadata ${assigned?.metadataId ?? ''}` : 'Click to add metadata JSON'}
                                onclick={() => openMetadataModal(dataset, column, assigned?.metadataId, validated)}
                            >
                                <span
                                    class="inline-block h-2 w-2 rounded-full"
                                    class:bg-green-500={hasMeta && Boolean(validated)}
                                    class:bg-yellow-500={hasMeta && !Boolean(validated)}
                                    class:bg-gray-300={!hasMeta}
                                ></span>
                                {hasMeta ? (validated ? 'valid' : 'set') : 'missing'}
                            </button>
                        </td>
                    {/each}
                </tr>
            {/each}
        </tbody>
    </table>
</div>

<MetadataAssignmentModal
    open={modalOpen}
    target={modalTarget}
    onClose={closeModal}
    onDataChanged={onDataChanged}
/>
