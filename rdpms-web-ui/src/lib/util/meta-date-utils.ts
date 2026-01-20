import type { DataSetSummaryDTO, MetaDateCollectionColumnDTO } from '$lib/api_client';

const normalizeKey = (value?: string | null) => (value ?? '').toLowerCase();

export const findAssignedMeta = (dataset: DataSetSummaryDTO, column: MetaDateCollectionColumnDTO) => {
    return dataset.metaDates?.find((meta) => normalizeKey(meta.metadataKey) === normalizeKey(column.metadataKey));
};
