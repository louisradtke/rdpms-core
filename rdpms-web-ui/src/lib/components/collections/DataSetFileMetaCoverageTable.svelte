<script lang="ts">
    import type { DataSetSummaryDTO, FileSummaryDTO, MetaDateCollectionColumnDTO } from '$lib/api_client';
    import { MetaDataRepository } from '$lib/data/MetaDataRepository';
    import { getOrFetchConfig, toApiConfig } from '$lib/util/config-helper';

    type DataSetWithFiles = DataSetSummaryDTO & { files?: FileSummaryDTO[] | null };

    let { datasets, columns, projectSlug, collectionSlug, onDataChanged } = $props<{
        datasets: DataSetSummaryDTO[];
        columns: MetaDateCollectionColumnDTO[];
        projectSlug: string;
        collectionSlug: string;
        onDataChanged: () => void;
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
    let metadataEditorOpen = $state(false);
    let metadataEditorPending = $state(false);
    let metadataEditorError = $state('');
    let metadataEditorJson = $state('{\n  \n}');
    let metadataEditorTitle = $state('');
    let metadataEditorKey = $state('');
    let metadataEditorFileId = $state('');
    let metadataEditorSchemaDbId = $state('');
    let metadataEditorSchemaId = $state('');

    const closeMetadataPopup = () => {
        metadataPopupOpen = false;
    };

    const closeMetadataEditor = () => {
        metadataEditorOpen = false;
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

    const openMetadataEditor = (
        title: string,
        fileId?: string | null,
        metadataKey?: string | null,
        schemaDbId?: string | null,
        schemaId?: string | null
    ) => {
        if (!fileId || !metadataKey) return;
        metadataEditorOpen = true;
        metadataEditorPending = false;
        metadataEditorError = '';
        metadataEditorJson = '{\n  \n}';
        metadataEditorTitle = title;
        metadataEditorKey = metadataKey;
        metadataEditorFileId = fileId;
        metadataEditorSchemaDbId = schemaDbId ?? '';
        metadataEditorSchemaId = schemaId ?? '';
    };

    const submitMetadataEditor = async () => {
        if (!metadataEditorFileId || !metadataEditorKey) {
            metadataEditorError = 'Missing target file or metadata key.';
            return;
        }

        try {
            JSON.parse(metadataEditorJson);
        } catch {
            metadataEditorError = 'Metadata JSON is not valid.';
            return;
        }

        metadataEditorPending = true;
        metadataEditorError = '';
        try {
            const repo = new MetaDataRepository(getOrFetchConfig().then(toApiConfig));
            await repo.setFileMetadata(metadataEditorFileId, metadataEditorKey, metadataEditorJson);
            closeMetadataEditor();
            onDataChanged();
        } catch (err) {
            metadataEditorError = err instanceof Error ? err.message : 'Failed to save metadata.';
        } finally {
            metadataEditorPending = false;
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
    <span class="ml-auto text-xs text-gray-500">Hint: click <span class="font-medium">missing</span> to add JSON or <span class="font-medium">set</span>/<span class="font-medium">valid</span> to view JSON.</span>
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
                                <span class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs bg-gray-100 text-gray-600" title="No file available to assign metadata">
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
                                {@const metadataKey = column.metadataKey ?? 'unknown'}
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
                                        <button
                                            type="button"
                                            class="inline-flex cursor-pointer items-center gap-1 rounded-full px-2 py-0.5 text-xs bg-gray-100 text-gray-600 hover:ring-1 hover:ring-gray-300"
                                            title="Click to add metadata JSON"
                                            onclick={() => openMetadataEditor(
                                                metadataTitle,
                                                file.id,
                                                metadataKey,
                                                column.schema?.id,
                                                column.schema?.schemaId
                                            )}
                                        >
                                            <span class="inline-block h-2 w-2 rounded-full bg-gray-300"></span>
                                            missing
                                        </button>
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

{#if metadataEditorOpen}
    <button
        class="fixed inset-0 z-40 bg-black/50"
        type="button"
        aria-label="Close metadata editor"
        onclick={closeMetadataEditor}
    ></button>
    <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="w-full max-w-3xl rounded-lg bg-white shadow-lg">
            <div class="flex items-center justify-between border-b px-4 py-3">
                <h2 class="text-lg font-semibold">Add Metadata JSON</h2>
                <button
                    class="text-gray-500 hover:text-gray-700"
                    type="button"
                    aria-label="Close"
                    onclick={closeMetadataEditor}
                >
                    ✕
                </button>
            </div>
            <div class="space-y-3 px-4 py-4">
                <p class="text-sm text-gray-600">{metadataEditorTitle}</p>
                <div class="rounded-md border border-blue-200 bg-blue-50 px-3 py-2 text-sm text-blue-900">
                    {#if metadataEditorSchemaDbId || metadataEditorSchemaId}
                        <p>
                            This metadata will be validated against schema
                            <span class="font-mono">{metadataEditorSchemaId || metadataEditorSchemaDbId}</span>.
                        </p>
                        {#if metadataEditorSchemaDbId}
                            <a
                                class="mt-2 inline-flex rounded-md border border-blue-300 bg-white px-2.5 py-1 text-xs hover:bg-blue-100"
                                href={`/schemas/${metadataEditorSchemaDbId}`}
                                target="_blank"
                                rel="noopener noreferrer"
                            >
                                Open schema in new tab
                            </a>
                        {/if}
                    {:else}
                        <p>No schema is configured for this metadata key, so no schema validation will run.</p>
                    {/if}
                </div>
                <textarea
                    class="h-64 w-full rounded-md border border-gray-300 p-3 font-mono text-sm"
                    bind:value={metadataEditorJson}
                    spellcheck={false}
                    placeholder="Paste metadata JSON"
                ></textarea>
                {#if metadataEditorError}
                    <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{metadataEditorError}</p>
                {/if}
                <div class="flex justify-end gap-2">
                    <button
                        class="rounded-md border border-gray-300 px-3 py-1.5 text-sm hover:bg-gray-50"
                        type="button"
                        onclick={closeMetadataEditor}
                        disabled={metadataEditorPending}
                    >
                        Cancel
                    </button>
                    <button
                        class="rounded-md bg-blue-600 px-3 py-1.5 text-sm text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-70"
                        type="button"
                        onclick={submitMetadataEditor}
                        disabled={metadataEditorPending}
                    >
                        {metadataEditorPending ? 'Saving...' : 'Save metadata'}
                    </button>
                </div>
            </div>
        </div>
    </div>
{/if}
