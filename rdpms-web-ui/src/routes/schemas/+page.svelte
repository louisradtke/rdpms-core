<script lang="ts">
    import LoadingCircle from '$lib/layout/LoadingCircle.svelte';
    import { SchemasRepository } from '$lib/data/SchemasRepository';
    import { getOrFetchConfig, toApiConfig } from '$lib/util/config-helper';

    let reloadTick = $state(0);

    let schemaIdInput = $state('');
    let schemaJsonInput = $state('');

    let createPending = $state(false);
    let createError = $state('');
    let createSuccess = $state('');

    let previewPending = $state(false);
    let previewError = $state('');
    let previewSchemaText = $state('');
    let previewSchemaDbId = $state('');

    const schemasPromise = $derived.by(async () => {
        void reloadTick; // make $derived.by read ot as dependency

        const repo = new SchemasRepository(getOrFetchConfig().then(toApiConfig));
        return repo.listSchemas();
    });

    async function createSchema(): Promise<void> {
        const raw = schemaJsonInput.trim();
        const schemaId = schemaIdInput.trim();

        if (!raw) {
            createError = 'Schema JSON is required.';
            createSuccess = '';
            return;
        }

        try {
            JSON.parse(raw);
        } catch {
            createError = 'Schema JSON is malformed.';
            createSuccess = '';
            return;
        }

        createPending = true;
        createError = '';
        createSuccess = '';

        try {
            const repo = new SchemasRepository(getOrFetchConfig().then(toApiConfig));
            await repo.addSchema(raw, schemaId || undefined);

            createSuccess = 'Schema has been added.';
            reloadTick += 1;
            schemaIdInput = '';
        } catch (err) {
            createError = err instanceof Error ? err.message : 'Failed to add schema.';
        } finally {
            createPending = false;
        }
    }

    async function loadSchemaPreview(schemaRef?: string | null): Promise<void> {
        if (!schemaRef) {
            previewError = 'Schema id is missing.';
            previewSchemaText = '';
            previewSchemaDbId = '';
            return;
        }

        previewPending = true;
        previewError = '';
        previewSchemaText = '';
        previewSchemaDbId = schemaRef;

        try {
            const repo = new SchemasRepository(getOrFetchConfig().then(toApiConfig));
            previewSchemaText = await repo.getSchemaRaw(schemaRef);
        } catch (err) {
            previewError = err instanceof Error ? err.message : 'Failed to load schema preview.';
        } finally {
            previewPending = false;
        }
    }
</script>

<main class="mx-auto my-5 w-full max-w-screen-xl px-4 sm:px-6 lg:px-8 space-y-6">
    <h1 class="text-2xl font-bold">Schemas</h1>

    <section class="w-full rounded-xl border border-gray-200 bg-white p-4 shadow-sm space-y-4">
        <h2 class="text-xl font-semibold">Add Schema</h2>

        {#if createError}
            <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{createError}</p>
        {/if}
        {#if createSuccess}
            <p class="rounded-md border border-green-200 bg-green-50 px-3 py-2 text-sm text-green-700">{createSuccess}</p>
        {/if}

        <label class="flex flex-col gap-1">
            <span class="text-sm font-medium">Schema ID (optional)</span>
            <input
                class="rounded-md border border-gray-300 px-3 py-2"
                placeholder="urn:rdpms:..."
                bind:value={schemaIdInput}
            />
            <span class="text-xs text-gray-500">
                Public schema identifier (URI/URN). Leave empty to let the backend generate one.
            </span>
        </label>

        <label class="flex flex-col gap-1">
            <span class="text-sm font-medium">Schema JSON</span>
            <textarea
                class="min-h-48 rounded-md border border-gray-300 px-3 py-2 font-mono text-sm"
                placeholder={'{"$schema":"https://json-schema.org/draft/2020-12/schema","type":"object"}'}
                bind:value={schemaJsonInput}
            ></textarea>
            <span class="text-xs text-gray-500">
                Raw JSON schema document. Must be valid JSON object/array and is stored as the schema content.
            </span>
        </label>

        <div>
            <button
                class="rounded-md bg-gray-800 px-3 py-2 text-sm text-white hover:bg-black/90 disabled:opacity-60"
                disabled={createPending}
                onclick={createSchema}
            >
                {createPending ? 'Adding...' : 'Add Schema'}
            </button>
        </div>
    </section>

    <section class="w-full rounded-xl border border-gray-200 bg-white p-4 shadow-sm space-y-4">
        <h2 class="text-xl font-semibold">Registered Schemas</h2>

        {#await schemasPromise}
            <div class="mt-3 flex justify-center">
                <LoadingCircle />
            </div>
        {:then schemas}
            {#if schemas.length === 0}
                <p class="text-sm text-gray-600">No schemas registered yet.</p>
            {:else}
                <div class="overflow-x-auto">
                    <table class="min-w-full text-sm">
                        <thead>
                            <tr class="border-b border-gray-200 text-left">
                                <th class="py-2 pr-4">DB ID</th>
                                <th class="py-2 pr-4">Schema ID</th>
                                <th class="py-2">Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            {#each schemas as schema, idx (`${schema.id ?? schema.schemaId ?? 'none'}-${idx}`)}
                                <tr class="border-b border-gray-100 align-top">
                                    <td class="py-2 pr-4 font-mono">{schema.id ?? '-'}</td>
                                    <td class="py-2 pr-4">{schema.schemaId ?? '-'}</td>
                                    <td class="py-2">
                                        <div class="flex items-center gap-2">
                                            <button
                                                class="rounded-md border border-gray-300 px-2 py-1 hover:bg-gray-50"
                                                onclick={() => loadSchemaPreview(schema.id ?? schema.schemaId)}
                                            >
                                                Preview JSON
                                            </button>
                                            {#if schema.id}
                                                <a
                                                    class="rounded-md border border-gray-300 px-2 py-1 hover:bg-gray-50"
                                                    href={`/schemas/${schema.id}`}
                                                >
                                                    Open Page
                                                </a>
                                            {/if}
                                        </div>
                                    </td>
                                </tr>
                            {/each}
                        </tbody>
                    </table>
                </div>
            {/if}
        {:catch err}
            <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-red-700">
                {err instanceof Error ? err.message : 'Failed to load schemas.'}
            </p>
        {/await}
    </section>

    <section class="w-full rounded-xl border border-gray-200 bg-white p-4 shadow-sm space-y-3">
        <h2 class="text-xl font-semibold">Schema Preview</h2>

        {#if previewError}
            <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{previewError}</p>
        {/if}

        {#if previewPending}
            <div class="mt-3 flex justify-center">
                <LoadingCircle />
            </div>
        {:else if previewSchemaText}
            <p class="text-sm text-gray-600">Previewing schema {previewSchemaDbId}</p>
            <pre class="max-h-96 overflow-auto rounded-md border border-gray-200 bg-gray-50 p-3 text-xs">{previewSchemaText}</pre>
        {:else}
            <p class="text-sm text-gray-600">Select a schema from the table to preview its JSON.</p>
        {/if}
    </section>
</main>
