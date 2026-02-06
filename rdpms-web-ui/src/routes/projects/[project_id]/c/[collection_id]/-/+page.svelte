<script lang="ts">
    import { page } from '$app/state';
    import LoadingCircle from '$lib/layout/LoadingCircle.svelte';
    import { CollectionsRepository } from '$lib/data/CollectionsRepository';
    import { SchemasRepository } from '$lib/data/SchemasRepository';
    import { getOrFetchConfig, toApiConfig } from '$lib/util/config-helper';

    let projectSlug = $derived(page.params.project_id ?? '');
    let collectionSlug = $derived(page.params.collection_id ?? '');

    let reloadTick = $state(0);

    let keyInput = $state('');
    let schemaIdInput = $state('');
    let newSchemaIdInput = $state('');
    let defaultMetadataIdInput = $state('');
    let newSchemaJsonInput = $state('');

    let savePending = $state(false);
    let saveError = $state('');
    let saveSuccess = $state('');

    const dataPromise = $derived.by(async () => {
        void reloadTick; // make $derived.by read it as dependency

        const configPromise = getOrFetchConfig().then(toApiConfig);
        const collectionsRepo = new CollectionsRepository(configPromise);
        const schemasRepo = new SchemasRepository(configPromise);

        const collections = await collectionsRepo.getCollections({ projectSlug });
        const collectionSummary = collections.find((entry) => entry.slug === collectionSlug || entry.id === collectionSlug);

        if (!collectionSummary?.id) {
            throw new Error(`Collection ${collectionSlug} not found in project ${projectSlug}.`);
        }

        const [collection, schemas] = await Promise.all([
            collectionsRepo.getCollectionById(collectionSummary.id),
            schemasRepo.listSchemas()
        ]);

        return { collection, schemas };
    });

    function fillFormFromExisting(
        metadataKey?: string | null,
        schemaId?: string | null,
        defaultFieldId?: string | null
    ): void {
        keyInput = metadataKey ?? '';
        schemaIdInput = schemaId ?? '';
        newSchemaIdInput = '';
        defaultMetadataIdInput = defaultFieldId ?? '';
        newSchemaJsonInput = '';
        saveSuccess = '';
        saveError = '';
    }

    async function submitColumn(collectionId: string): Promise<void> {
        const key = keyInput.trim();
        if (!key) {
            saveError = 'Metadata key is required.';
            saveSuccess = '';
            return;
        }

        savePending = true;
        saveError = '';
        saveSuccess = '';

        try {
            let selectedSchemaDbId = schemaIdInput.trim();
            const newSchemaJson = newSchemaJsonInput.trim();
            const newSchemaId = newSchemaIdInput.trim();

            if (newSchemaJson) {
                JSON.parse(newSchemaJson);

                const schemasRepo = new SchemasRepository(getOrFetchConfig().then(toApiConfig));
                const before = await schemasRepo.listSchemas();
                const beforeIds = new Set(before.map((schema) => schema.id).filter((id): id is string => Boolean(id)));

                await schemasRepo.addSchema(newSchemaJson, newSchemaId || undefined);

                const after = await schemasRepo.listSchemas();
                const created = newSchemaId
                    ? after.find((schema) => schema.schemaId === newSchemaId)
                    : after.find((schema) => schema.id && !beforeIds.has(schema.id));

                if (!created?.id) {
                    throw new Error('Could not resolve the newly registered schema id.');
                }

                selectedSchemaDbId = created.id;
            }

            if (!selectedSchemaDbId) {
                throw new Error('Select a registered schema or provide JSON to register a new one.');
            }

            const collectionsRepo = new CollectionsRepository(getOrFetchConfig().then(toApiConfig));
            await collectionsRepo.upsertMetadataColumn(collectionId, key, {
                schemaId: selectedSchemaDbId,
                defaultMetadataId: defaultMetadataIdInput.trim() || undefined,
            });

            saveSuccess = `Schema binding for key "${key}" has been saved.`;
            schemaIdInput = selectedSchemaDbId;
            newSchemaIdInput = '';
            newSchemaJsonInput = '';
            reloadTick += 1;
        } catch (err) {
            saveError = err instanceof Error ? err.message : 'Failed to save schema binding.';
        } finally {
            savePending = false;
        }
    }
</script>

<main class="mx-auto my-5 w-full max-w-screen-xl px-4 sm:px-6 lg:px-8 space-y-6">
    <h1 class="text-2xl font-bold">Collection Settings</h1>

    {#await dataPromise}
        <div class="mt-3 flex justify-center">
            <LoadingCircle />
        </div>
    {:then data}
        <section class="w-full rounded-xl border border-gray-200 bg-white p-4 shadow-sm space-y-4">
            <div class="flex items-center justify-between">
                <h2 class="text-xl font-semibold">Schemas</h2>
                <p class="text-sm text-gray-600">Collection: {data.collection.name ?? data.collection.slug ?? data.collection.id}</p>
            </div>

            <p class="text-sm text-gray-600">
                Add or update metadata schema mappings by key. Select an existing registered schema, or register a new
                schema below and bind it immediately. Re-using an existing key overwrites its settings.
            </p>

            {#if saveError}
                <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{saveError}</p>
            {/if}
            {#if saveSuccess}
                <p class="rounded-md border border-green-200 bg-green-50 px-3 py-2 text-sm text-green-700">{saveSuccess}</p>
            {/if}

            <div class="grid grid-cols-1 gap-3 md:grid-cols-2">
                <label class="flex flex-col gap-1">
                    <span class="text-sm font-medium">Metadata key</span>
                    <input
                        class="rounded-md border border-gray-300 px-3 py-2"
                        placeholder="e.g. viz"
                        bind:value={keyInput}
                    />
                </label>

                <div class="flex min-w-0 items-end gap-2">
                    <label class="flex min-w-0 flex-1 flex-col gap-1">
                        <span class="text-sm font-medium">Schema (registered)</span>
                        <select class="w-full min-w-0 rounded-md border border-gray-300 px-3 py-2" bind:value={schemaIdInput}>
                            <option value="">None</option>
                            {#each data.schemas as schema (schema.id)}
                                <option value={schema.id ?? ''}>{schema.schemaId ?? schema.id}</option>
                            {/each}
                        </select>
                        <span class="text-xs text-gray-500">
                            Uses the schema DB id for the metadata-key binding.
                        </span>
                    </label>
                    <a
                        class={`mb-0 inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-md border border-gray-300 text-sm ${schemaIdInput ? 'hover:bg-gray-50' : 'pointer-events-none opacity-40'}`}
                        href={schemaIdInput ? `/schemas/${schemaIdInput}` : undefined}
                        title={schemaIdInput ? 'Open selected schema preview page' : 'Select a schema to preview it'}
                        aria-label="Open selected schema preview page"
                    >
                        ↗
                    </a>
                </div>

                <label class="flex flex-col gap-1 md:col-span-2">
                    <span class="text-sm font-medium">Default metadata id (optional)</span>
                    <input
                        class="rounded-md border border-gray-300 px-3 py-2"
                        placeholder="GUID of metadata blob"
                        bind:value={defaultMetadataIdInput}
                    />
                </label>

                <label class="flex flex-col gap-1 md:col-span-2">
                    <span class="text-sm font-medium">New schema ID (optional)</span>
                    <input
                        class="rounded-md border border-gray-300 px-3 py-2"
                        placeholder="Schema URI/URN to register (e.g. urn:rdpms:...)"
                        bind:value={newSchemaIdInput}
                    />
                    <span class="text-xs text-gray-500">
                        If filled with JSON below, the UI first registers this schema, then binds it to the key.
                    </span>
                </label>

                <label class="flex flex-col gap-1 md:col-span-2">
                    <span class="text-sm font-medium">New schema JSON (optional)</span>
                    <textarea
                        class="min-h-36 rounded-md border border-gray-300 px-3 py-2 font-mono text-sm"
                        placeholder={'{"$schema":"https://json-schema.org/draft/2020-12/schema"}'}
                        bind:value={newSchemaJsonInput}
                    ></textarea>
                    <span class="text-xs text-gray-500">
                        Must be a valid JSON object/array. This creates a schema first; it does not send inline schema to the collection endpoint.
                    </span>
                </label>
            </div>

            <div class="flex items-center gap-3">
                <button
                    class="rounded-md bg-gray-800 px-3 py-2 text-sm text-white hover:bg-black/90 disabled:opacity-60"
                    disabled={savePending}
                    onclick={() => submitColumn(data.collection.id ?? '')}
                >
                    {savePending ? 'Saving...' : 'Save Mapping'}
                </button>
            </div>

            <div class="pt-2">
                <h3 class="text-lg font-semibold">Current Mappings</h3>

                {#if (data.collection.metaDateColumns?.length ?? 0) === 0}
                    <p class="mt-2 text-sm text-gray-600">No schema mappings configured yet.</p>
                {:else}
                    <div class="mt-2 overflow-x-auto">
                        <table class="min-w-full text-sm">
                            <thead>
                                <tr class="border-b border-gray-200 text-left">
                                    <th class="py-2 pr-4">Key</th>
                                    <th class="py-2 pr-4">Schema</th>
                                    <th class="py-2 pr-4">Default</th>
                                    <th class="py-2">Action</th>
                                </tr>
                            </thead>
                        <tbody>
                                {#each data.collection.metaDateColumns ?? [] as column, idx (`${column.metadataKey ?? column.schema?.id ?? 'none'}-${idx}`)}
                                    <tr class="border-b border-gray-100 align-top">
                                        <td class="py-2 pr-4 font-mono">{column.metadataKey ?? '-'}</td>
                                        <td class="py-2 pr-4">{column.schema?.schemaId ?? column.schema?.id ?? '-'}</td>
                                        <td class="py-2 pr-4 font-mono">{column.defaultFieldId ?? '-'}</td>
                                        <td class="py-2">
                                            <button
                                                class="rounded-md border border-gray-300 px-2 py-1 hover:bg-gray-50"
                                                onclick={() => fillFormFromExisting(column.metadataKey, column.schema?.id, column.defaultFieldId)}
                                            >
                                                Edit in form
                                            </button>
                                        </td>
                                    </tr>
                                {/each}
                            </tbody>
                        </table>
                    </div>
                {/if}
            </div>
        </section>
    {:catch err}
        <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-red-700">
            {err instanceof Error ? err.message : 'Failed to load schema settings.'}
        </p>
    {/await}
</main>
