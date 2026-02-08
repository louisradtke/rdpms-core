<script lang="ts">
    import type { DataSetSummaryDTO } from "$lib/api_client";
    import { DataSetsRepository } from "$lib/data/DataSetsRepository";
    import { getOrFetchConfig, toApiConfig } from "$lib/util/config-helper";

    let { datasets, projectSlug, collectionSlug, onDelete } = $props<{
        datasets: DataSetSummaryDTO[];
        projectSlug: string;
        collectionSlug: string;
        onDelete: () => void;
    }>();

    const formatDate = (value?: Date | null) => value ? value.toLocaleString() : '—';

    let deletingId = $state<string | null>(null);
    let deleteError = $state('');

    async function deleteDataset(dataset: DataSetSummaryDTO): Promise<void> {
        if (!dataset.id) return;
        const confirmed = window.confirm(`Delete dataset "${dataset.name ?? dataset.id}"?`);
        if (!confirmed) return;

        deletingId = dataset.id;
        deleteError = '';

        try {
            const repo = new DataSetsRepository(getOrFetchConfig().then(toApiConfig));
            await repo.deleteById(dataset.id);
            onDelete();
        } catch (e) {
            deleteError = e instanceof Error ? e.message : 'Failed to delete dataset.';
        } finally {
            deletingId = null;
        }
    }
</script>

{#if deleteError}
    <p class="mb-2 rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{deleteError}</p>
{/if}

<table class="table-fixed w-full">
    <thead>
    <tr>
        <th class="text-left w-48">Dataset Name</th>
        <th>Create date</th>
        <th>Begin</th>
        <th>End</th>
        <th>Files</th>
        <th>Time series</th>
        <th class="text-right w-10"></th>
    </tr>
    </thead>

    <tbody>
        {#each datasets as dataset (dataset.id)}
            <tr>
                <td class="text-left">
                    <a href="/projects/{projectSlug}/c/{collectionSlug}/{dataset.slug ?? dataset.id}"
                       class="text-blue-500 hover:underline">
                        {dataset.name}
                    </a>
                </td>
                <td>{formatDate(dataset.createdStampUTC)}</td>
                <td>{formatDate(dataset.beginStampUTC)}</td>
                <td>{formatDate(dataset.endStampUTC)}</td>
                <td>{dataset.fileCount ?? 0}</td>
                <td>{dataset.isTimeSeries ? 'yes' : 'no'}</td>
                <td class="text-right">
                    <button
                            class="px-3 py-1 rounded bg-red-600 text-white hover:bg-red-700 disabled:opacity-50"
                            aria-label="Delete dataset"
                            disabled={deletingId === dataset.id}
                            onclick={() => deleteDataset(dataset)}
                    >
                        {deletingId === dataset.id ? 'Deleting...' : 'Delete'}
                    </button>
                </td>
            </tr>
        {/each}
    </tbody>
</table>
