import type { FileSummaryDTO } from "$lib/api_client";

export const CORE_PLUGIN_IDS = {
    image: "rdpms.image",
    table: "rdpms.table",
    pdf: "rdpms.pdf",
    code: "rdpms.code",
    gpsTrackSvg: "rdpms.gps-track-svg",
    gpsTrackOsm: "rdpms.gps-track-osm",
    timeSeriesPlotly: "rdpms.timeseries-plotly"
} as const;

export type CorePluginId = (typeof CORE_PLUGIN_IDS)[keyof typeof CORE_PLUGIN_IDS];

export interface PluginDownloadPolicy {
    maxBytes: number;
    description: string;
    overrideLabel?: string;
    strategy?: "block" | "truncate";
}

export interface PluginDisplayPolicy {
    maxBytes?: number;
    oversizeMessage?: string;
    rowPreviewLimit?: number;
}

export interface CorePluginPolicy {
    download?: PluginDownloadPolicy;
    display?: PluginDisplayPolicy;
}

const MEBIBYTE = 1024 * 1024;

const corePluginPolicies: Partial<Record<CorePluginId, CorePluginPolicy>> = {
    [CORE_PLUGIN_IDS.table]: {
        download: {
            maxBytes: 10 * MEBIBYTE,
            description:
                "Table previews download the full file into the browser before rendering it.",
            overrideLabel: "Download anyway"
        },
        display: {
            maxBytes: 10 * MEBIBYTE,
            oversizeMessage: "File too large for table preview (max 10 MiB)",
            rowPreviewLimit: 500
        }
    },
    [CORE_PLUGIN_IDS.code]: {
        download: {
            maxBytes: 2 * MEBIBYTE,
            description: "Code previews load only the first 2 MiB of the file in the browser.",
            overrideLabel: "Download anyway",
            strategy: "truncate"
        },
        display: {
            maxBytes: 2 * MEBIBYTE,
            oversizeMessage: "File too large for inline preview (max 2 MiB)"
        }
    },
    [CORE_PLUGIN_IDS.gpsTrackSvg]: {
        download: {
            maxBytes: 2 * MEBIBYTE,
            description: "GPS previews download and parse the full CSV file in the browser.",
            overrideLabel: "Download anyway"
        },
        display: {
            maxBytes: 2 * MEBIBYTE,
            oversizeMessage: "File too large for GPS preview (max 2 MiB)"
        }
    },
    [CORE_PLUGIN_IDS.timeSeriesPlotly]: {
        download: {
            maxBytes: 25 * MEBIBYTE,
            description:
                "Time-series plots download and parse the full CSV file in the browser before rendering it.",
            overrideLabel: "Download anyway"
        },
        display: {
            maxBytes: 25 * MEBIBYTE,
            oversizeMessage: "File too large for Plotly time-series preview (max 25 MiB)"
        }
    },
    [CORE_PLUGIN_IDS.gpsTrackOsm]: {
        download: {
            maxBytes: 25 * MEBIBYTE,
            description:
                "OpenStreetMap previews download and parse the full CSV file in the browser before rendering it.",
            overrideLabel: "Download anyway"
        },
        display: {
            maxBytes: 25 * MEBIBYTE,
            oversizeMessage: "File too large for OpenStreetMap preview (max 25 MiB)"
        }
    }
};

const TEXT_ABBREVIATIONS = new Set([
    "txt",
    "md",
    "markdown",
    "json",
    "yaml",
    "yml",
    "xml",
    "html",
    "css",
    "js",
    "ts",
    "py",
    "cpp",
    "c",
    "h",
    "hpp",
    "ini",
    "toml",
    "log",
    "csv"
]);

const MIME_TEXT_MATCHES = [
    "application/json",
    "application/xml",
    "application/yaml",
    "application/javascript",
    "application/x-javascript",
    "application/x-sh",
    "application/x-python-code"
];

const normalize = (value?: string | null) => (value ?? "").toLowerCase();

export function isSupportedCorePluginId(id: string): id is CorePluginId {
    return Object.values(CORE_PLUGIN_IDS).includes(id as CorePluginId);
}

export function getCorePluginPolicy(id: CorePluginId): CorePluginPolicy | undefined {
    return corePluginPolicies[id];
}

export function autoPluginCandidates(file: FileSummaryDTO): CorePluginId[] {
    const mime = normalize(file.contentType?.mimeType);
    const abbreviation = normalize(file.contentType?.abbreviation);

    if (
        mime.startsWith("image/") ||
        ["png", "jpg", "jpeg", "gif", "webp", "bmp", "svg"].includes(abbreviation)
    ) {
        return [CORE_PLUGIN_IDS.image];
    }

    if (mime === "application/pdf" || abbreviation === "pdf") {
        return [CORE_PLUGIN_IDS.pdf];
    }

    if (
        mime === "text/csv" ||
        mime === "application/csv" ||
        ["csv", "tsv"].includes(abbreviation)
    ) {
        if (file.isTimeSeries) {
            return [CORE_PLUGIN_IDS.timeSeriesPlotly, CORE_PLUGIN_IDS.table, CORE_PLUGIN_IDS.code];
        }
        return [CORE_PLUGIN_IDS.table, CORE_PLUGIN_IDS.code];
    }

    if (isTextLike(mime, abbreviation)) {
        return [CORE_PLUGIN_IDS.code];
    }

    return [];
}

function isTextLike(mime: string, abbreviation: string): boolean {
    if (mime.startsWith("text/")) {
        return true;
    }

    if (MIME_TEXT_MATCHES.includes(mime)) {
        return true;
    }

    return TEXT_ABBREVIATIONS.has(abbreviation);
}
