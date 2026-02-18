# Visualization Plugin Registry

This page documents the currently available core visualization plugins in the web UI, including their IDs and selection behavior.

## Core Visualization Plugins

| Plugin | ID | Component | Typical auto-selection conditions | Mentionable details |
| --- | --- | --- | --- | --- |
| Image | `rdpms.image` | `ImagePlugin.svelte` | MIME starts with `image/` or abbreviation in `png`, `jpg`, `jpeg`, `gif`, `webp`, `bmp`, `svg` | Renders an `<img>` with the file `downloadURI`. |
| Table | `rdpms.table` | `TablePlugin.svelte` | CSV-like inputs (`text/csv`, `application/csv`, abbreviation `csv` or `tsv`) | CSV preview with loading/error states and sortable columns (`svelte-table`); file-size limit is 10 MB. |
| PDF | `rdpms.pdf` | `PdfPlugin.svelte` | MIME `application/pdf` or abbreviation `pdf` | Renders an `<iframe>` preview, with fallback text if URI is unavailable. |
| Code | `rdpms.code` | `CodePlugin.svelte` | Text-like MIME/abbreviation; also included as a secondary candidate for CSV/TSV | Inline text preview with loading/error states; file-size limit is 2 MB. |

## Selection and Fallback Behavior

- Core plugin IDs are declared in `rdpms-web-ui/src/lib/layout/displayPlugins/plugin-registry.ts`.
- If `preferredPluginIds` are provided and valid, they are used as selectable plugins.
- If no valid preferred plugins are provided, the UI derives candidates automatically from MIME type and content-type abbreviation.
- For CSV/TSV data, the candidate order is `rdpms.table` first, then `rdpms.code`.
- The initial selection can be overridden by `preferredDefaultPluginId` when it is valid and available.
- If `includeHiddenMode` is enabled in `FileDisplay.svelte`, a `Hidden` mode is added to the mode switch.

## Related Sources

- `rdpms-web-ui/src/lib/layout/displayPlugins/plugin-registry.ts`
- `rdpms-web-ui/src/lib/layout/FileDisplay.svelte`
- `rdpms-web-ui/src/lib/layout/displayPlugins/ImagePlugin.svelte`
- `rdpms-web-ui/src/lib/layout/displayPlugins/TablePlugin.svelte`
- `rdpms-web-ui/src/lib/layout/displayPlugins/PdfPlugin.svelte`
- `rdpms-web-ui/src/lib/layout/displayPlugins/CodePlugin.svelte`
