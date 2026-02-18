<script lang="ts">
    import type { SchemaValidationResultDTO } from '$lib/api_client';
    import { MetaDataRepository } from '$lib/data/MetaDataRepository';
    import { getOrFetchConfig, toApiConfig } from '$lib/util/config-helper';
    import type { MetadataAssignmentTarget } from '$lib/components/collections/metadata-modal-types';

    // Shared requirements for refactors:
    // 1) This modal must remain the single metadata editor for both dataset and file coverage tables.
    // 2) Save/Validate/Save&Validate button state rules must stay enforced in this component.
    // 3) Save and validation must never auto-close the modal; only explicit close actions may close it.
    let { open, target, onClose, onDataChanged } = $props<{
        open: boolean;
        target: MetadataAssignmentTarget | null;
        onClose: () => void;
        onDataChanged: () => void;
    }>();

    let loading = $state(false);
    let actionPending = $state(false);
    let actionLabel = $state('');
    let errorMessage = $state('');
    let infoMessage = $state('');
    let editorJson = $state('{\n  \n}');
    let originalJsonCanonical = $state<string | null>(JSON.stringify({}));
    let originalEditorJson = $state('{\n  \n}');
    let currentMetadataId = $state('');
    let currentValidated = $state(false);
    let validationResult = $state<SchemaValidationResultDTO | null>(null);
    let lastLoadedTargetKey = $state('');
    let pendingDataRefresh = $state(false);
    let loadVersion = 0;

    const schemaValidationId = $derived(target?.schemaDbId ?? '');
    const schemaDisplayId = $derived(target?.schemaId ?? target?.schemaDbId ?? '');

    const canonicalizeJson = (value: string): string | null => {
        try {
            return JSON.stringify(JSON.parse(value));
        } catch {
            return null;
        }
    };

    const asRecord = (value: unknown): Record<string, unknown> | null => {
        if (!value || typeof value !== 'object' || Array.isArray(value)) {
            return null;
        }
        return value as Record<string, unknown>;
    };

    const toStringArray = (value: unknown): string[] => {
        if (!Array.isArray(value)) {
            return [];
        }
        return value.filter((entry): entry is string => typeof entry === 'string');
    };

    const toSchemaValidationResult = (value: unknown): SchemaValidationResultDTO | null => {
        const record = asRecord(value);
        if (!record) {
            return null;
        }

        const hasFlag = typeof record.succesful === 'boolean' || record.succesful === null;
        const reasons = toStringArray(record.reasons);
        const traces = toStringArray(record.traces);
        if (!hasFlag && reasons.length === 0 && traces.length === 0) {
            return null;
        }

        return {
            succesful: hasFlag ? (record.succesful as boolean | null) : null,
            reasons,
            traces
        };
    };

    const readErrorDetails = async (err: unknown, fallback: string): Promise<{
        message: string;
        validation: SchemaValidationResultDTO | null;
    }> => {
        const errRecord = asRecord(err);
        const response = errRecord?.response instanceof Response ? errRecord.response : null;
        if (!response) {
            return {
                message: err instanceof Error ? err.message : fallback,
                validation: null
            };
        }

        let payload: unknown = null;
        try {
            payload = await response.clone().json();
        } catch {
            try {
                payload = await response.clone().text();
            } catch {
                payload = null;
            }
        }

        const validation = toSchemaValidationResult(payload);
        if (validation) {
            const firstReason = (validation.reasons ?? [])[0];
            return {
                message: firstReason ? `Validation failed: ${firstReason}` : 'Validation failed.',
                validation
            };
        }

        const payloadRecord = asRecord(payload);
        const payloadMessage = typeof payload === 'string'
            ? payload
            : typeof payloadRecord?.message === 'string'
                ? payloadRecord.message
                : typeof payloadRecord?.detail === 'string'
                    ? payloadRecord.detail
                    : null;

        if (payloadMessage && payloadMessage.trim() !== '') {
            return { message: payloadMessage, validation: null };
        }

        return {
            message: err instanceof Error ? err.message : `${fallback} (HTTP ${response.status})`,
            validation: null
        };
    };

    const editorJsonCanonical = $derived(canonicalizeJson(editorJson));
    const editorJsonValid = $derived(editorJsonCanonical !== null);
    const jsonChanged = $derived.by(() => {
        if (editorJsonCanonical !== null && originalJsonCanonical !== null) {
            return editorJsonCanonical !== originalJsonCanonical;
        }
        return editorJson !== originalEditorJson;
    });

    const validateDisabled = $derived(
        actionPending ||
            loading ||
            !currentMetadataId ||
            !schemaValidationId ||
            currentValidated ||
            jsonChanged
    );
    const saveDisabled = $derived(actionPending || loading || !editorJsonValid || !jsonChanged);
    const saveAndValidateDisabled = $derived(
        actionPending || loading || !editorJsonValid || !jsonChanged || !schemaValidationId
    );

    const requestClose = () => {
        if (pendingDataRefresh) {
            onDataChanged();
            pendingDataRefresh = false;
        }
        onClose();
    };

    const initializeModal = async (nextTarget: MetadataAssignmentTarget) => {
        const targetKey = [
            nextTarget.targetType,
            nextTarget.targetId,
            nextTarget.metadataKey,
            nextTarget.metadataId ?? '',
            nextTarget.schemaId ?? '',
            nextTarget.schemaDbId ?? ''
        ].join('::');

        if (targetKey === lastLoadedTargetKey && open) {
            return;
        }
        lastLoadedTargetKey = targetKey;

        const currentLoadVersion = ++loadVersion;
        loading = true;
        errorMessage = '';
        infoMessage = '';
        validationResult = null;
        actionPending = false;
        actionLabel = '';
        editorJson = '{\n  \n}';
        originalEditorJson = editorJson;
        originalJsonCanonical = JSON.stringify({});

        currentMetadataId = nextTarget.metadataId ?? '';
        currentValidated = Boolean(nextTarget.validated);

        if (!nextTarget.metadataId) {
            editorJson = '{\n  \n}';
            originalEditorJson = editorJson;
            originalJsonCanonical = JSON.stringify({});
            loading = false;
            return;
        }

        try {
            const repo = new MetaDataRepository(getOrFetchConfig().then(toApiConfig));
            const metadata = await repo.getById(nextTarget.metadataId);
            if (currentLoadVersion !== loadVersion) {
                return;
            }

            currentMetadataId = metadata.id ?? nextTarget.metadataId;

            if (!metadata.fileId) {
                throw new Error('Metadata file reference is missing.');
            }

            const jsonValue = await repo.getJsonValueByFileId(metadata.fileId);
            if (currentLoadVersion !== loadVersion) {
                return;
            }

            editorJson = JSON.stringify(jsonValue, null, 2);
            originalEditorJson = editorJson;
            originalJsonCanonical = canonicalizeJson(editorJson);
        } catch (err) {
            if (currentLoadVersion !== loadVersion) {
                return;
            }
            errorMessage = err instanceof Error ? err.message : 'Failed to load metadata.';
        } finally {
            if (currentLoadVersion === loadVersion) {
                loading = false;
            }
        }
    };

    $effect(() => {
        if (!open || !target) {
            return;
        }
        void initializeModal(target);
    });

    $effect(() => {
        if (!open) {
            lastLoadedTargetKey = '';
            pendingDataRefresh = false;
        }
    });

    const saveMetadata = async (): Promise<boolean> => {
        if (!target) {
            errorMessage = 'Missing metadata target.';
            return false;
        }
        if (editorJsonCanonical === null) {
            errorMessage = 'Metadata JSON is not valid.';
            return false;
        }

        actionPending = true;
        actionLabel = 'Saving...';
        errorMessage = '';
        infoMessage = '';

        try {
            const repo = new MetaDataRepository(getOrFetchConfig().then(toApiConfig));
            const saveResult =
                target.targetType === 'dataset'
                    ? await repo.setDatasetMetadata(target.targetId, target.metadataKey, editorJson)
                    : await repo.setFileMetadata(target.targetId, target.metadataKey, editorJson);

            currentMetadataId = saveResult.id ?? currentMetadataId;
            currentValidated = false;
            originalEditorJson = editorJson;
            originalJsonCanonical = editorJsonCanonical;
            validationResult = null;
            infoMessage = 'Metadata saved.';
            pendingDataRefresh = true;
            return true;
        } catch (err) {
            const details = await readErrorDetails(err, 'Failed to save metadata.');
            errorMessage = details.message;
            return false;
        } finally {
            actionPending = false;
            actionLabel = '';
        }
    };

    const validateMetadata = async (): Promise<boolean> => {
        if (!currentMetadataId) {
            errorMessage = 'No metadata is assigned yet. Save first to validate.';
            return false;
        }
        if (!schemaValidationId) {
            errorMessage = 'No schema is configured for this metadata key.';
            return false;
        }

        actionPending = true;
        actionLabel = 'Validating...';
        errorMessage = '';
        infoMessage = '';

        try {
            const repo = new MetaDataRepository(getOrFetchConfig().then(toApiConfig));
            const result = await repo.validateMetadata(currentMetadataId, schemaValidationId, true);

            validationResult = result;
            currentValidated = Boolean(result.succesful);
            if (result.succesful) {
                infoMessage = 'Validation successful.';
            } else {
                infoMessage = '';
                errorMessage = 'Validation failed. See reasons below.';
            }
            pendingDataRefresh = true;
            return Boolean(result.succesful);
        } catch (err) {
            const details = await readErrorDetails(err, 'Validation failed.');
            if (details.validation) {
                validationResult = details.validation;
                currentValidated = Boolean(details.validation.succesful);
            }
            errorMessage = details.message;
            return false;
        } finally {
            actionPending = false;
            actionLabel = '';
        }
    };

    const saveAndValidate = async () => {
        const saveOk = await saveMetadata();
        if (!saveOk) {
            return;
        }
        await validateMetadata();
    };
</script>

{#if open && target}
    <button
        class="fixed inset-0 z-40 bg-black/50"
        type="button"
        aria-label="Close metadata editor"
        onclick={requestClose}
    ></button>
    <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="flex h-[94vh] w-[96vw] max-w-[1800px] flex-col rounded-lg bg-white shadow-lg">
            <div class="flex items-center justify-between border-b px-4 py-3">
                <h2 class="text-lg font-semibold">Metadata JSON</h2>
                <button
                    class="text-gray-500 hover:text-gray-700"
                    type="button"
                    aria-label="Close"
                    onclick={requestClose}
                >
                    ✕
                </button>
            </div>

            <div class="grid min-h-0 flex-1 grid-rows-[minmax(0,1fr)_minmax(0,34vh)] gap-4 overflow-hidden px-4 py-4 xl:grid-cols-[minmax(0,1.35fr)_minmax(360px,1fr)] xl:grid-rows-1">
                <div class="flex min-h-0 flex-col gap-3 overflow-hidden">
                    <p class="text-sm text-gray-600">{target.title}</p>
                    <dl class="grid grid-cols-1 gap-1 rounded-md border border-gray-200 bg-gray-50 px-3 py-2 text-xs text-gray-700 sm:grid-cols-2">
                        <div>
                            <dt class="font-semibold">Target</dt>
                            <dd class="font-mono">{target.targetType} / {target.targetId}</dd>
                        </div>
                        <div>
                            <dt class="font-semibold">Metadata key</dt>
                            <dd class="font-mono">{target.metadataKey}</dd>
                        </div>
                        <div>
                            <dt class="font-semibold">Metadata ID</dt>
                            <dd class="font-mono break-all">{currentMetadataId || '-'}</dd>
                        </div>
                        <div>
                            <dt class="font-semibold">Collection schema</dt>
                            <dd class="font-mono break-all">{schemaDisplayId || '-'}</dd>
                        </div>
                    </dl>

                    <div class="rounded-md border border-blue-200 bg-blue-50 px-3 py-2 text-sm text-blue-900">
                        {#if schemaValidationId}
                            <p>
                                Validation uses schema <span class="font-mono">{schemaDisplayId || schemaValidationId}</span>.
                            </p>
                            {#if target.schemaDbId}
                                <a
                                    class="mt-2 inline-flex rounded-md border border-blue-300 bg-white px-2.5 py-1 text-xs hover:bg-blue-100"
                                    href={`/schemas/${target.schemaDbId}`}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                >
                                    Open schema in new tab
                                </a>
                            {/if}
                        {:else}
                            <p>No schema is configured for this metadata key, so validation cannot run.</p>
                        {/if}
                    </div>

                    {#if loading}
                        <p class="text-sm text-gray-600">Loading metadata...</p>
                    {/if}

                    <textarea
                        class="min-h-0 flex-1 rounded-md border border-gray-300 p-3 font-mono text-sm"
                        bind:value={editorJson}
                        spellcheck={false}
                        placeholder="Paste metadata JSON"
                        disabled={loading || actionPending}
                    ></textarea>

                    {#if !editorJsonValid}
                        <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
                            Metadata JSON is not valid.
                        </p>
                    {/if}
                </div>

                <div class="flex min-h-0 flex-col overflow-hidden rounded-md border border-gray-200 bg-gray-50 p-3">
                    <h3 class="mb-2 text-sm font-semibold text-gray-800">Validation Output</h3>
                    <div class="min-h-0 flex-1 space-y-3 overflow-auto pr-1">
                        {#if infoMessage}
                            <p class="break-all rounded-md border border-green-200 bg-green-50 px-3 py-2 text-sm text-green-700">{infoMessage}</p>
                        {/if}

                        {#if errorMessage}
                            <p class="break-all rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{errorMessage}</p>
                        {/if}

                        {#if validationResult}
                            {@const reasons = validationResult.reasons ?? []}
                            {@const traces = validationResult.traces ?? []}
                            <div
                                class="space-y-2 rounded-md border px-3 py-2 text-sm"
                                class:border-green-200={validationResult.succesful}
                                class:bg-green-50={validationResult.succesful}
                                class:text-green-900={validationResult.succesful}
                                class:border-red-200={!validationResult.succesful}
                                class:bg-red-50={!validationResult.succesful}
                                class:text-red-900={!validationResult.succesful}
                            >
                                <p class="font-semibold">
                                    {validationResult.succesful ? 'Validation succeeded.' : 'Validation failed.'}
                                </p>

                                {#if reasons.length > 0}
                                    <div>
                                        <p class="font-semibold">Reasons</p>
                                        <ul class="list-disc space-y-1 pl-5">
                                            {#each reasons as reason, index (`reason-${index}`)}
                                                <li class="font-mono text-xs break-all">{reason}</li>
                                            {/each}
                                        </ul>
                                    </div>
                                {/if}

                                {#if traces.length > 0}
                                    <div>
                                        <p class="font-semibold">Traces</p>
                                        <pre class="mt-1 overflow-x-auto rounded-md border border-gray-300 bg-white p-2 text-xs">{traces.join('\n')}</pre>
                                    </div>
                                {/if}
                            </div>
                        {:else}
                            <p class="text-sm text-gray-500">No validation results yet.</p>
                        {/if}
                    </div>
                </div>
            </div>

            <div class="flex flex-wrap justify-end gap-2 border-t px-4 py-3">
                <button
                    class="rounded-md border border-gray-300 px-3 py-1.5 text-sm hover:bg-gray-50"
                    type="button"
                    onclick={requestClose}
                    disabled={actionPending}
                >
                    Close
                </button>
                <button
                    class="rounded-md border border-gray-300 px-3 py-1.5 text-sm hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-70"
                    type="button"
                    onclick={validateMetadata}
                    disabled={validateDisabled}
                >
                    {actionPending && actionLabel === 'Validating...' ? actionLabel : 'Validate'}
                </button>
                <button
                    class="rounded-md bg-blue-600 px-3 py-1.5 text-sm text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-70"
                    type="button"
                    onclick={saveMetadata}
                    disabled={saveDisabled}
                >
                    {actionPending && actionLabel === 'Saving...' ? actionLabel : 'Save'}
                </button>
                <button
                    class="rounded-md bg-emerald-600 px-3 py-1.5 text-sm text-white hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-70"
                    type="button"
                    onclick={saveAndValidate}
                    disabled={saveAndValidateDisabled}
                >
                    {actionPending && actionLabel !== '' ? actionLabel : 'Save & validate'}
                </button>
            </div>
        </div>
    </div>
{/if}
