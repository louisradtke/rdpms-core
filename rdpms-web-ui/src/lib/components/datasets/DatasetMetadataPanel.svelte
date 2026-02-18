<script lang="ts">
    import {
        MetadataColumnTargetDTO,
        type AssignedMetaDateDTO,
        type DataSetSummaryDTO,
        type MetaDateCollectionColumnDTO
    } from '$lib/api_client';
    import MetadataAssignmentModal from '$lib/components/collections/MetadataAssignmentModal.svelte';
    import type { MetadataAssignmentTarget } from '$lib/components/collections/metadata-modal-types';

    let {
        dataset,
        columns = [],
        onDataChanged
    } = $props<{
        dataset: DataSetSummaryDTO;
        columns?: MetaDateCollectionColumnDTO[] | null;
        onDataChanged: () => void;
    }>();

    const normalizeKey = (value?: string | null) => (value ?? '').toLowerCase();

    const datasetColumns = $derived(
        (columns ?? []).filter((column: MetaDateCollectionColumnDTO) => column.target === MetadataColumnTargetDTO.Dataset)
    );

    const effectiveColumns = $derived.by(() => {
        if (datasetColumns.length > 0) {
            return datasetColumns;
        }

        return (dataset.metaDates ?? []).map((meta: AssignedMetaDateDTO) => ({
            metadataKey: meta.metadataKey ?? 'unknown'
        } as MetaDateCollectionColumnDTO));
    });

    const findAssigned = (column: MetaDateCollectionColumnDTO) =>
        dataset.metaDates?.find((entry: AssignedMetaDateDTO) => normalizeKey(entry.metadataKey) === normalizeKey(column.metadataKey));

    let modalOpen = $state(false);
    let modalTarget = $state<MetadataAssignmentTarget | null>(null);

    const openModal = (column: MetaDateCollectionColumnDTO) => {
        const datasetId = dataset.id ?? '';
        if (!datasetId) {
            return;
        }

        const metadataKey = column.metadataKey ?? 'unknown';
        const assigned = findAssigned(column);

        modalTarget = {
            targetType: 'dataset',
            targetId: datasetId,
            title: `${dataset.name ?? dataset.id ?? 'Dataset'} / ${metadataKey}`,
            metadataKey,
            metadataId: assigned?.metadataId,
            schemaDbId: column.schema?.id,
            schemaId: column.schema?.schemaId,
            validated: assigned?.collectionSchemaVerified
        };
        modalOpen = true;
    };

    const closeModal = () => {
        modalOpen = false;
    };
</script>

<section class="space-y-3">
    <h2 class="text-lg font-semibold text-gray-800">Dataset Metadata</h2>

    {#if effectiveColumns.length === 0}
        <p class="rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-600">No metadata keys configured.</p>
    {:else}
        <div class="flex flex-wrap gap-2">
            {#each effectiveColumns as column (column.metadataKey)}
                {@const assigned = findAssigned(column)}
                {@const hasMeta = Boolean(assigned)}
                {@const validated = assigned?.collectionSchemaVerified}
                <button
                    type="button"
                    class="inline-flex cursor-pointer items-center gap-1 rounded-full px-2.5 py-1 text-xs hover:ring-1 hover:ring-gray-300"
                    class:bg-green-100={hasMeta && Boolean(validated)}
                    class:text-green-800={hasMeta && Boolean(validated)}
                    class:bg-yellow-100={hasMeta && !Boolean(validated)}
                    class:text-yellow-800={hasMeta && !Boolean(validated)}
                    class:bg-gray-100={!hasMeta}
                    class:text-gray-600={!hasMeta}
                    onclick={() => openModal(column)}
                    title={hasMeta ? `Open metadata ${assigned?.metadataId ?? ''}` : 'Add metadata'}
                >
                    <span
                        class="inline-block h-2 w-2 rounded-full"
                        class:bg-green-500={hasMeta && Boolean(validated)}
                        class:bg-yellow-500={hasMeta && !Boolean(validated)}
                        class:bg-gray-300={!hasMeta}
                    ></span>
                    <span class="font-mono">{column.metadataKey ?? 'unknown'}</span>
                    <span>{hasMeta ? (validated ? 'valid' : 'set') : 'missing'}</span>
                </button>
            {/each}
        </div>
    {/if}
</section>

<MetadataAssignmentModal
    open={modalOpen}
    target={modalTarget}
    onClose={closeModal}
    onDataChanged={onDataChanged}
/>
