<script lang="ts">
    import CodeCopyField from "$lib/layout/CodeCopyField.svelte";
    interface Entity {
        id?: string | null,
        slug?: string | null,
        name?: string | null,
        description?: string | null
    }

    export let type: string;
    export let entity: Entity;
</script>

<p class="text-xs text-gray-600 mt-1">{(type ?? "").toUpperCase()}</p>
<h1 class="text-2xl font-bold">{entity.name}</h1>
<div class="flex gap-4">
    <CodeCopyField text={entity.slug ?? ""}/>
    <CodeCopyField text={entity.id ?? ""}/>
</div>

{#if entity.description}
    <div>
        <p>{entity.description}</p>
    </div>
{/if}
