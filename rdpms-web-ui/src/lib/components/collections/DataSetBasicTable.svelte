<script lang="ts">
    import type { DataSetSummaryDTO } from "$lib/api_client";

    let { datasets, projectSlug, collectionSlug } = $props<{
        datasets: DataSetSummaryDTO[];
        projectSlug: string;
        collectionSlug: string;
    }>();

    const formatDate = (value?: Date | null) => value ? value.toLocaleString() : '—';
</script>

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
                            class="px-3 py-1 rounded bg-blue-600 text-white hover:bg-blue-700"
                            aria-label="Edit project"
                    >
                        Edit
                    </button>
                </td>
            </tr>
        {/each}
    </tbody>
</table>
