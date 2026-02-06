<script lang="ts">
    import { page } from '$app/state';
    import LoadingCircle from '$lib/layout/LoadingCircle.svelte';
    import { SchemasRepository } from '$lib/data/SchemasRepository';
    import { getOrFetchConfig, toApiConfig } from '$lib/util/config-helper';

    const schemaDbId = $derived(page.params.schema_id ?? '');

    const schemaPromise = $derived.by(async () => {
        void schemaDbId;
        if (!schemaDbId) {
            throw new Error('Missing schema id.');
        }

        const repo = new SchemasRepository(getOrFetchConfig().then(toApiConfig));
        const [schemas, raw] = await Promise.all([
            repo.listSchemas(),
            repo.getSchemaRaw(schemaDbId)
        ]);

        const schema = schemas.find((entry) => entry.id === schemaDbId);

        let formatted = raw;
        try {
            const parsed = JSON.parse(raw);
            formatted = JSON.stringify(parsed, null, 2);
        } catch {
            // Keep raw response when payload is not valid JSON text.
        }

        return { schema, raw, formatted };
    });
</script>

<main class="w-full px-4 py-5 sm:px-6 lg:px-8 space-y-4">
    <div class="flex items-center justify-between">
        <h1 class="text-2xl font-bold">Schema</h1>
        <a class="rounded-md border border-gray-300 px-3 py-2 text-sm hover:bg-gray-50" href="/schemas">
            Back to Schemas
        </a>
    </div>

    {#await schemaPromise}
        <div class="mt-3 flex justify-center">
            <LoadingCircle />
        </div>
    {:then data}
        <section class="rounded-xl border border-gray-200 bg-white p-4 shadow-sm space-y-3">
            <p class="text-sm text-gray-600">DB ID: <span class="font-mono">{schemaDbId}</span></p>
            <p class="text-sm text-gray-600">Schema ID: {data.schema?.schemaId ?? '-'}</p>
            <pre class="h-[calc(100vh-16rem)] overflow-auto rounded-md border border-gray-200 bg-gray-50 p-4 text-xs">{data.formatted}</pre>
        </section>
    {:catch err}
        <p class="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-red-700">
            {err instanceof Error ? err.message : 'Failed to load schema.'}
        </p>
    {/await}
</main>
