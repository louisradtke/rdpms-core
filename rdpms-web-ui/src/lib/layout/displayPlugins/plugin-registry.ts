import type { FileSummaryDTO } from '$lib/api_client';

export const CORE_PLUGIN_IDS = {
    image: 'rdpms.image',
    table: 'rdpms.table',
    pdf: 'rdpms.pdf',
    code: 'rdpms.code',
    gpsTrackSvg: 'rdpms.gps-track-svg'
} as const;

export type CorePluginId = (typeof CORE_PLUGIN_IDS)[keyof typeof CORE_PLUGIN_IDS];

const TEXT_ABBREVIATIONS = new Set([
    'txt',
    'md',
    'markdown',
    'json',
    'yaml',
    'yml',
    'xml',
    'html',
    'css',
    'js',
    'ts',
    'py',
    'cpp',
    'c',
    'h',
    'hpp',
    'ini',
    'toml',
    'log',
    'csv'
]);

const MIME_TEXT_MATCHES = [
    'application/json',
    'application/xml',
    'application/yaml',
    'application/javascript',
    'application/x-javascript',
    'application/x-sh',
    'application/x-python-code'
];

const normalize = (value?: string | null) => (value ?? '').toLowerCase();

export function isSupportedCorePluginId(id: string): id is CorePluginId {
    return Object.values(CORE_PLUGIN_IDS).includes(id as CorePluginId);
}

export function autoPluginCandidates(file: FileSummaryDTO): CorePluginId[] {
    const mime = normalize(file.contentType?.mimeType);
    const abbreviation = normalize(file.contentType?.abbreviation);

    if (mime.startsWith('image/') || ['png', 'jpg', 'jpeg', 'gif', 'webp', 'bmp', 'svg'].includes(abbreviation)) {
        return [CORE_PLUGIN_IDS.image];
    }

    if (mime === 'application/pdf' || abbreviation === 'pdf') {
        return [CORE_PLUGIN_IDS.pdf];
    }

    if (
        mime === 'text/csv' ||
        mime === 'application/csv' ||
        ['csv', 'tsv'].includes(abbreviation)
    ) {
        return [CORE_PLUGIN_IDS.table, CORE_PLUGIN_IDS.code];
    }

    if (isTextLike(mime, abbreviation)) {
        return [CORE_PLUGIN_IDS.code];
    }

    return [];
}

function isTextLike(mime: string, abbreviation: string): boolean {
    if (mime.startsWith('text/')) {
        return true;
    }

    if (MIME_TEXT_MATCHES.includes(mime)) {
        return true;
    }

    return TEXT_ABBREVIATIONS.has(abbreviation);
}
