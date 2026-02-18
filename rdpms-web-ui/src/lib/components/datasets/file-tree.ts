import type { FileSummaryDTO } from '$lib/api_client';

export type FileTreeFolderNode = {
    type: 'folder';
    name: string;
    path: string;
    children: FileTreeNode[];
};

export type FileTreeFileNode = {
    type: 'file';
    name: string;
    path: string;
    file: FileSummaryDTO;
};

export type FileTreeNode = FileTreeFolderNode | FileTreeFileNode;

type InternalFolder = {
    folders: Map<string, InternalFolder>;
    files: FileTreeFileNode[];
};

const normalizeSegments = (file: FileSummaryDTO): string[] => {
    const raw = (file.name ?? file.id ?? 'unnamed').replace(/\\/g, '/');
    return raw.split('/').map((segment) => segment.trim()).filter((segment) => segment.length > 0);
};

const toPublicNodes = (folder: InternalFolder, pathPrefix = ''): FileTreeNode[] => {
    const entries: FileTreeNode[] = [];

    const folderEntries = Array.from(folder.folders.entries()).sort(([left], [right]) => left.localeCompare(right));
    for (const [name, child] of folderEntries) {
        const path = pathPrefix ? `${pathPrefix}/${name}` : name;
        entries.push({
            type: 'folder',
            name,
            path,
            children: toPublicNodes(child, path)
        });
    }

    const fileEntries = [...folder.files].sort((left, right) => {
        const leftName = left.name.toLowerCase();
        const rightName = right.name.toLowerCase();
        return leftName.localeCompare(rightName);
    });

    entries.push(...fileEntries);
    return entries;
};

export const buildFileTree = (files: FileSummaryDTO[]): FileTreeNode[] => {
    const root: InternalFolder = { folders: new Map(), files: [] };

    for (const file of files) {
        const segments = normalizeSegments(file);
        if (segments.length === 0) {
            root.files.push({
                type: 'file',
                name: file.name ?? file.id ?? 'unnamed',
                path: file.name ?? file.id ?? 'unnamed',
                file
            });
            continue;
        }

        let current = root;
        let path = '';
        for (let index = 0; index < segments.length - 1; index += 1) {
            const segment = segments[index];
            path = path ? `${path}/${segment}` : segment;
            if (!current.folders.has(segment)) {
                current.folders.set(segment, { folders: new Map(), files: [] });
            }
            current = current.folders.get(segment)!;
        }

        const leafName = segments[segments.length - 1];
        const leafPath = path ? `${path}/${leafName}` : leafName;
        current.files.push({
            type: 'file',
            name: leafName,
            path: leafPath,
            file
        });
    }

    return toPublicNodes(root);
};
