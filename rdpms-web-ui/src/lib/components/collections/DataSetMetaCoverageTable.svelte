<script lang="ts">
    import type { DataSetSummaryDTO, MetaDateCollectionColumnDTO } from "$lib/api_client";
    import { findAssignedMeta } from "$lib/util/meta-date-utils";

    let { datasets, columns, projectSlug, collectionSlug } = $props<{
        datasets: DataSetSummaryDTO[];
        columns: MetaDateCollectionColumnDTO[];
        projectSlug: string;
        collectionSlug: string;
    }>();
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
                        <a href="/projects/{projectSlug}/c/{collectionSlug}/{dataset.slug ?? dataset.id}"
                           class="text-blue-500 hover:underline">
                            {dataset.name}
                        </a>
                    </td>
                    {#each columns as column (column.metadataKey)}
                        {@const assigned = findAssignedMeta(dataset, column)}
                        {@const hasMeta = Boolean(assigned)}
                        {@const validated = assigned?.collectionSchemaVerified}
                        <td class="text-center">
                            <span
                                class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs"
                                class:bg-green-100={hasMeta && validated}
                                class:text-green-800={hasMeta && validated}
                                class:bg-yellow-100={hasMeta && !validated}
                                class:text-yellow-800={hasMeta && !validated}
                                class:bg-gray-100={!hasMeta}
                                class:text-gray-600={!hasMeta}
                                title={assigned?.metadataId ?? 'missing'}
                            >
                                <span
                                    class="inline-block h-2 w-2 rounded-full"
                                    class:bg-green-500={hasMeta && validated}
                                    class:bg-yellow-500={hasMeta && !validated}
                                    class:bg-gray-300={!hasMeta}
                                ></span>
                                {#if hasMeta}
                                    {validated ? 'valid' : 'set'}
                                {:else}
                                    missing
                                {/if}
                            </span>
                        </td>
                    {/each}
                </tr>
            {/each}
        </tbody>
    </table>
</div>
