<script lang="ts">
    import type {FileSummaryDTO} from "$lib/api_client";
	import ImagePlugin from "./displayPlugins/ImagePlugin.svelte";
    import {FileSize} from "$lib/data/FileSize";
    import CodeCopyField from "$lib/layout/CodeCopyField.svelte";
    import TablePlugin from "$lib/layout/displayPlugins/TablePlugin.svelte";
    import PdfPlugin from "$lib/layout/displayPlugins/PdfPlugin.svelte";
    import CodePlugin from "$lib/layout/displayPlugins/CodePlugin.svelte";
    import {autoPluginCandidates, CORE_PLUGIN_IDS, isSupportedCorePluginId, type CorePluginId} from "$lib/layout/displayPlugins/plugin-registry";

    type DisplayMode = CorePluginId | 'hidden';

    function downloadName(file: FileSummaryDTO): string {
        if (file.name) return file.name;
        if (file.contentType?.abbreviation) return file.id + '.' + file.contentType.abbreviation;
        return "unknown";
    }

    async function handleDownload(event: Event, file: FileSummaryDTO) {
        event.preventDefault();
        
        if (!file.downloadURI) return;
        
        try {
            const response = await fetch(file.downloadURI);
            if (!response.ok) throw new Error('Download failed');
            
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = downloadName(file);
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error('Download failed:', error);
            // Fallback to opening in new tab
            window.open(file.downloadURI, '_blank');
        }
    }

    let {
        title,
        fileSlug,
        file,
        preferredPluginIds = [],
        preferredDefaultPluginId,
        includeHiddenMode = false,
        defaultDisplayMode = 'auto'
    }: {
        title: string;
        fileSlug: string;
        file: FileSummaryDTO;
        preferredPluginIds?: string[];
        preferredDefaultPluginId?: string;
        includeHiddenMode?: boolean;
        defaultDisplayMode?: 'auto' | 'hidden';
    } = $props();

    let size = $derived(!Number.isNaN(file?.size) ? new FileSize(file.size || 0) : undefined);

    const pluginLabels: Record<DisplayMode, string> = {
        hidden: 'Hidden',
        [CORE_PLUGIN_IDS.image]: 'Image',
        [CORE_PLUGIN_IDS.table]: 'Table',
        [CORE_PLUGIN_IDS.pdf]: 'PDF',
        [CORE_PLUGIN_IDS.code]: 'Code'
    };

    const collectSelectablePlugins = (preferredIds: string[], fallbackFile: FileSummaryDTO): CorePluginId[] => {
        const normalized = preferredIds
            .map((id) => id.trim())
            .filter((id) => id.length > 0);

        const result: CorePluginId[] = [];
        for (const pluginId of normalized) {
            if (isSupportedCorePluginId(pluginId) && !result.includes(pluginId)) {
                result.push(pluginId);
            }
        }

        if (result.length > 0) {
            return result;
        }

        const autoCandidates = autoPluginCandidates(fallbackFile);
        for (const pluginId of autoCandidates) {
            if (!result.includes(pluginId)) {
                result.push(pluginId);
            }
        }

        return result;
    };

    const resolveInitialPlugin = (
        pluginIds: CorePluginId[],
        defaultPluginId?: string,
        hiddenEnabled = false,
        preferredMode: 'auto' | 'hidden' = 'auto'
    ): DisplayMode | null => {
        if (hiddenEnabled && preferredMode === 'hidden') {
            return 'hidden';
        }

        if (defaultPluginId && isSupportedCorePluginId(defaultPluginId) && pluginIds.includes(defaultPluginId)) {
            return defaultPluginId;
        }

        return pluginIds[0] ?? null;
    };

    let selectablePluginIds = $derived.by(() => collectSelectablePlugins(preferredPluginIds, file));
    let selectableModes = $derived.by((): DisplayMode[] => {
        const modes: DisplayMode[] = [];
        if (includeHiddenMode) {
            modes.push('hidden');
        }
        modes.push(...selectablePluginIds);
        return modes;
    });
    let userSelectedPluginId = $state<DisplayMode | null>(null);
    let visitedPluginIds = $state<CorePluginId[]>([]);
    let selectedPluginId = $derived.by(() => {
        if (userSelectedPluginId && selectableModes.includes(userSelectedPluginId)) {
            return userSelectedPluginId;
        }

        return resolveInitialPlugin(selectablePluginIds, preferredDefaultPluginId, includeHiddenMode, defaultDisplayMode);
    });

    const isSelected = (pluginId: DisplayMode): boolean => selectedPluginId === pluginId;
    const isMounted = (pluginId: CorePluginId): boolean => isSelected(pluginId) || visitedPluginIds.includes(pluginId);

    const selectPlugin = (pluginId: DisplayMode): void => {
        const nextVisited = [...visitedPluginIds];
        if (selectedPluginId && selectedPluginId !== 'hidden' && !nextVisited.includes(selectedPluginId)) {
            nextVisited.push(selectedPluginId);
        }
        if (pluginId !== 'hidden' && !nextVisited.includes(pluginId)) {
            nextVisited.push(pluginId);
        }
        visitedPluginIds = nextVisited;
        userSelectedPluginId = pluginId;
    };
</script>

<div id="{fileSlug}">

    <section class="overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm">
        <header class="flex items-center justify-between border-b border-gray-200 bg-gray-50 px-4 py-2">
            <h2 class="font-medium text-gray-800">{title}</h2>
            <div class="flex items-center gap-4">
                {#if selectableModes.length > 0}
                    <div class="inline-flex overflow-hidden rounded-md border border-gray-300 bg-white">
                        {#each selectableModes as pluginId (pluginId)}
                            <button
                                type="button"
                                class={`cursor-pointer px-2 py-1 text-xs ${isSelected(pluginId) ? 'bg-gray-700 text-white' : 'bg-white text-gray-700 hover:bg-gray-100'} ${pluginId !== selectableModes[0] ? 'border-l border-gray-300' : ''}`}
                                onclick={() => selectPlugin(pluginId)}
                            >
                                {pluginLabels[pluginId]}
                            </button>
                        {/each}
                    </div>
                {/if}
                <span class="text-xs text-gray-500">{size !== undefined ? size.getString('*B', true, 1) : 'undefined'}</span>
                <button onclick={(e) => handleDownload(e, file)}
                   class="text-gray-500 hover:text-gray-700 cursor-pointer" aria-label="Download">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24"
                         stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                              d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
                    </svg>
                </button>
                <CodeCopyField text={file.downloadURI ?? ''} copyOnly={true} tooltip="Copy download URI" />
            </div>
        </header>
        {#if selectedPluginId !== 'hidden'}
        <div class="p-4">
            {#if selectablePluginIds.includes(CORE_PLUGIN_IDS.image) && isMounted(CORE_PLUGIN_IDS.image)}
                <div class={isSelected(CORE_PLUGIN_IDS.image) ? 'block' : 'hidden'}>
                    <ImagePlugin uri={file.downloadURI ?? ''} />
                </div>
            {/if}
            {#if selectablePluginIds.includes(CORE_PLUGIN_IDS.table) && isMounted(CORE_PLUGIN_IDS.table)}
                <div class={isSelected(CORE_PLUGIN_IDS.table) ? 'block' : 'hidden'}>
                    <TablePlugin dataUri={file.downloadURI ?? ''} />
                </div>
            {/if}
            {#if selectablePluginIds.includes(CORE_PLUGIN_IDS.pdf) && isMounted(CORE_PLUGIN_IDS.pdf)}
                <div class={isSelected(CORE_PLUGIN_IDS.pdf) ? 'block' : 'hidden'}>
                    <PdfPlugin uri={file.downloadURI ?? ''} />
                </div>
            {/if}
            {#if selectablePluginIds.includes(CORE_PLUGIN_IDS.code) && isMounted(CORE_PLUGIN_IDS.code)}
                <div class={isSelected(CORE_PLUGIN_IDS.code) ? 'block' : 'hidden'}>
                    <CodePlugin dataUri={file.downloadURI ?? ''} />
                </div>
            {/if}
            {#if !selectedPluginId}
                <p class="text-center text-sm text-gray-500">File type not supported</p>
            {/if}
        </div>
        {/if}
    </section>

</div>
