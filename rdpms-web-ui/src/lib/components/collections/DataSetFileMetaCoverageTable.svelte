<script lang="ts">
    import type { DataSetSummaryDTO, FileSummaryDTO, MetaDateCollectionColumnDTO } from '$lib/api_client';
    import { MetaDataRepository } from '$lib/data/MetaDataRepository';
    import { getOrFetchConfig, toApiConfig } from '$lib/util/config-helper';

    type DataSetWithFiles = DataSetSummaryDTO & { files?: FileSummaryDTO[] | null };

    let { datasets, columns, projectSlug, collectionSlug } = $props<{
        datasets: DataSetSummaryDTO[];
        columns: MetaDateCollectionColumnDTO[];
        projectSlug: string;
        collectionSlug: string;
    }>();

    const datasetsWithFiles = $derived((datasets as DataSetWithFiles[]));

    const normalizeKey = (value?: string | null) => (value ?? '').toLowerCase();
    const findAssignedMeta = (file: FileSummaryDTO, column: MetaDateCollectionColumnDTO) => {
        return file.metaDates?.find((meta) => normalizeKey(meta.metadataKey) === normalizeKey(column.metadataKey));
    };

    let metadataPopupOpen = $state(false);
    let metadataPopupPending = $state(false);
    let metadataPopupError = $state('');
    let metadataPopupJson = $state('');
    let metadataPopupTitle = $state('');
    let metadataPopupMetadataId = $state('');
    let metadataPopupFileId = $state('');

    const closeMetadataPopup = () => {
        metadataPopupOpen = false;
    };

    const openMetadataPopup = async (title: string, metadataId?: string | null) => {
        if (!metadataId) return;

        metadataPopupOpen = true;
        metadataPopupPending = true;
        metadataPopupError = '';
        metadataPopupJson = '';
        metadataPopupTitle = title;
        metadataPopupMetadataId = metadataId;
        metadataPopupFileId = '';

        try {
            const repo = new MetaDataRepository(getOrFetchConfig().then(toApiConfig));
            const metadata = await repo.getById(metadataId);

            if (!metadata.fileId) {
                metadataPopupError = 'Metadata file reference is missing.';
                return;
            }

            metadataPopupFileId = metadata.fileId;
            const metadataJson = await repo.getJsonValueByFileId(metadata.fileId);
            metadataPopupJson = JSON.stringify(metadataJson, null, 2);
        } catch (err) {
            metadataPopupError = err instanceof Error ? err.message : 'Failed to load metadata.';
        } finally {
            metadataPopupPending = false;
        }
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
    <span class="ml-auto text-xs text-gray-500">Hint: click <span class="font-medium">set</span> or <span class="font-medium">valid</span> to view JSON.</span>
</div>
<div class="overflow-x-auto">
    <table class="table-auto w-full">
        <thead>
            <tr>
                <th class="text-left w-48">Dataset</th>
                <th class="text-left w-56">File</th>
                {#each columns as column (column.metadataKey)}
                    <th class="text-center min-w-[140px]" title={column.schema?.schemaId ?? column.schema?.id ?? ''}>
                        {column.metadataKey ?? 'unknown'}
                    </th>
                {/each}
            </tr>
        </thead>
        <tbody>
            {#each datasetsWithFiles as dataset (dataset.id)}
                <tr class="bg-gray-50">
                    <td class="font-semibold">
                        <a href="/projects/{projectSlug}/c/{collectionSlug}/{dataset.slug ?? dataset.id}" class="text-blue-500 hover:underline">
                            {dataset.name}
                        </a>
                    </td>
                    <td colspan={Math.max(columns.length + 1, 1)} class="text-xs text-gray-500">
                        {(dataset.files?.length ?? 0)} file{(dataset.files?.length ?? 0) === 1 ? '' : 's'}
                    </td>
                </tr>
                {#if (dataset.files?.length ?? 0) === 0}
                    <tr>
                        <td></td>
                        <td class="text-sm text-gray-500 italic">No files</td>
                        {#each columns as column (column.metadataKey)}
                            <td class="text-center">
                                <span class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs bg-gray-100 text-gray-600">
                                    <span class="inline-block h-2 w-2 rounded-full bg-gray-300"></span>
                                    missing
                                </span>
                            </td>
                        {/each}
                    </tr>
                {:else}
                    {#each dataset.files ?? [] as file (file.id)}
                        <tr>
                            <td></td>
                            <td class="text-left">{file.name ?? file.id}</td>
                            {#each columns as column (column.metadataKey)}
                                {@const assigned = findAssignedMeta(file, column)}
                                {@const hasMeta = Boolean(assigned)}
                                {@const validated = assigned?.collectionSchemaVerified}
                                {@const metadataTitle = `${dataset.name ?? dataset.id ?? 'Dataset'} / ${file.name ?? file.id ?? 'File'} / ${column.metadataKey ?? 'unknown'}`}
                                <td class="text-center">
                                    {#if hasMeta}
                                        <button
                                            type="button"
                                            class="inline-flex cursor-pointer items-center gap-1 rounded-full px-2 py-0.5 text-xs hover:ring-1 hover:ring-gray-300"
                                            class:bg-green-100={validated}
                                            class:text-green-800={validated}
                                            class:bg-yellow-100={!validated}
                                            class:text-yellow-800={!validated}
                                            title={`Open metadata ${assigned?.metadataId ?? ''}`}
                                            onclick={() => openMetadataPopup(metadataTitle, assigned?.metadataId)}
                                        >
                                            <span
                                                class="inline-block h-2 w-2 rounded-full"
                                                class:bg-green-500={validated}
                                                class:bg-yellow-500={!validated}
                                            ></span>
                                            {validated ? 'valid' : 'set'}
                                        </button>
                                    {:else}
                                        <span class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs bg-gray-100 text-gray-600" title="missing">
                                            <span class="inline-block h-2 w-2 rounded-full bg-gray-300"></span>
                                            missing
                                        </span>
                                    {/if}
                                </td>
                            {/each}
                        </tr>
                    {/each}
                {/if}
            {/each}
        </tbody>
    </table>
</div>

{#if metadataPopupOpen}
    <button
        class="fixed inset-0 z-40 bg-black/50"
        type="button"
        aria-label="Close metadata preview"
        onclick={closeMetadataPopup}
    ></button>
    <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="w-full max-w-3xl rounded-lg bg-white shadow-lg">
            <div class="flex items-center justify-between border-b px-4 py-3">
                <h2 class="text-lg font-semibold">Metadata JSON</h2>
                <button
                    class="text-gray-500 hover:text-gray-700"
                    type="button"
                    aria-label="Close"
                    onclick={closeMetadataPopup}
                >
                    ✕
                </button>
            </div>
            <div class="space-y-3 px-4 py-4">
                <p class="text-sm text-gray-600">{metadataPopupTitle}</p>
                <dl class="grid grid-cols-1 gap-1 rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-xs text-gray-700 sm:grid-cols-2">
                    <div>
                        <dt class="font-semibold">Metadata ID</dt>
                        <dd class="font-mono break-all">{metadataPopupMetadataId || '-'}</dd>
                    </div>
                    <div>
                        <dt class="font-semibold">Backend File ID</dt>
                        <dd class="font-mono break-all">{metadataPopupFileId || '-'}</dd>
                    </div>
                </dl>
                {#if metadataPopupPending}
                    <p class="text-sm text-gray-600">Loading metadata...</p>
                {:else if metadataPopupError}
                    <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{metadataPopupError}</p>
                {:else}
                    <pre class="max-h-[60vh] overflow-auto rounded-md border border-gray-200 bg-gray-50 p-3 text-xs">{metadataPopupJson}</pre>
                {/if}
            </div>
        </div>
    </div>
{/if}
