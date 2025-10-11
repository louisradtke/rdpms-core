<script lang="ts">
    // let { text = "" } = $props();
    export let text: string;
    export let copyOnly = false;
    export let tooltip: string = "copy value";
</script>

{#if text}
    <div class="relative inline-flex items-center">
        {#if !copyOnly}
            <div class="rounded-md bg-gray-100 px-1">
                <code class="text-gray-700 text-[0.65em]">{text}</code>
            </div>
            <div class="px-1"></div>
        {/if}
        <button class="flex items-center justify-center py-1  cursor-pointer"
                onclick={() => navigator.clipboard.writeText(text ?? "")}
                title={tooltip}
                aria-label="Copy text to clipboard">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 text-gray-500 hover:text-gray-700" fill="none"
                 viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
            </svg>
        </button>
    </div>
{/if}