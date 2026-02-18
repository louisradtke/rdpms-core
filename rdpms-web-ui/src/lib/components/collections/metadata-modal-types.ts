export type MetadataAssignmentTarget = {
    targetType: 'dataset' | 'file';
    targetId: string;
    title: string;
    metadataKey: string;
    metadataId?: string | null;
    schemaDbId?: string | null;
    schemaId?: string | null;
    validated?: boolean | null;
};
