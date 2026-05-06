import { DeletionStateDTO, type DataSetSummaryDTO } from "$lib/api_client";

export const DATASET_SORT_KEYS = [
    "name",
    "begin",
    "end",
    "duration",
    "created",
    "fileCount",
    "totalSizeBytes",
    "deletionState"
] as const;

export type DatasetSortKey = (typeof DATASET_SORT_KEYS)[number];
export type SortDirection = "asc" | "desc";

export type DatasetListQuery = {
    sort: DatasetSortKey;
    dir: SortDirection;
    beginMin: string;
    beginMax: string;
    endMin: string;
    endMax: string;
    durationMin: string;
    durationMax: string;
    fileCountMin: string;
    fileCountMax: string;
    deletionState: string[];
};

export const DEFAULT_DATASET_LIST_QUERY: DatasetListQuery = {
    sort: "begin",
    dir: "desc",
    beginMin: "",
    beginMax: "",
    endMin: "",
    endMax: "",
    durationMin: "",
    durationMax: "",
    fileCountMin: "",
    fileCountMax: "",
    deletionState: []
};

export const DATASET_QUERY_PARAM_KEYS = [
    "dssort",
    "dsdir",
    "dsbeginMin",
    "dsbeginMax",
    "dsendMin",
    "dsendMax",
    "dsdurationMin",
    "dsdurationMax",
    "dsfileCountMin",
    "dsfileCountMax",
    "dsdeletionState"
] as const;

export type DatasetQueryParamKey = (typeof DATASET_QUERY_PARAM_KEYS)[number];

const isDatasetSortKey = (value: string | null): value is DatasetSortKey => {
    return DATASET_SORT_KEYS.includes(value as DatasetSortKey);
};

const isSortDirection = (value: string | null): value is SortDirection => {
    return value === "asc" || value === "desc";
};

const getNumberFilter = (value: string) => {
    if (value.trim() === "") return null;
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
};

export const parseDatasetListQuery = (params: URLSearchParams): DatasetListQuery => {
    const sort = params.get("dssort");
    const dir = params.get("dsdir");
    const deletionStates = [
        ...new Set(
            params
                .getAll("dsdeletionState")
                .filter((value) =>
                    Object.values(DeletionStateDTO).includes(value as DeletionStateDTO)
                )
        )
    ];

    return {
        sort: isDatasetSortKey(sort) ? sort : DEFAULT_DATASET_LIST_QUERY.sort,
        dir: isSortDirection(dir) ? dir : DEFAULT_DATASET_LIST_QUERY.dir,
        beginMin: params.get("dsbeginMin") ?? "",
        beginMax: params.get("dsbeginMax") ?? "",
        endMin: params.get("dsendMin") ?? "",
        endMax: params.get("dsendMax") ?? "",
        durationMin: params.get("dsdurationMin") ?? "",
        durationMax: params.get("dsdurationMax") ?? "",
        fileCountMin: params.get("dsfileCountMin") ?? "",
        fileCountMax: params.get("dsfileCountMax") ?? "",
        deletionState: deletionStates
    };
};

export const writeDatasetListQuery = (url: URL, query: DatasetListQuery): URL => {
    const next = new URL(url);
    const params = next.searchParams;

    const setOrDelete = (key: DatasetQueryParamKey, value: string, defaultValue = "") => {
        if (value === defaultValue || value.trim() === "") {
            params.delete(key);
            return;
        }

        params.set(key, value);
    };

    setOrDelete("dssort", query.sort, DEFAULT_DATASET_LIST_QUERY.sort);
    setOrDelete("dsdir", query.dir, DEFAULT_DATASET_LIST_QUERY.dir);
    setOrDelete("dsbeginMin", query.beginMin);
    setOrDelete("dsbeginMax", query.beginMax);
    setOrDelete("dsendMin", query.endMin);
    setOrDelete("dsendMax", query.endMax);
    setOrDelete("dsdurationMin", query.durationMin);
    setOrDelete("dsdurationMax", query.durationMax);
    setOrDelete("dsfileCountMin", query.fileCountMin);
    setOrDelete("dsfileCountMax", query.fileCountMax);
    params.delete("dsdeletionState");
    for (const state of query.deletionState) {
        params.append("dsdeletionState", state);
    }

    return next;
};

export const preserveDatasetListParams = (target: URL, source: URLSearchParams): URL => {
    for (const key of DATASET_QUERY_PARAM_KEYS) {
        target.searchParams.delete(key);
        for (const value of source.getAll(key)) {
            if (value !== "") {
                target.searchParams.append(key, value);
            }
        }
    }

    return target;
};

export const hasDatasetFilters = (query: DatasetListQuery) => {
    return (
        query.beginMin !== "" ||
        query.beginMax !== "" ||
        query.endMin !== "" ||
        query.endMax !== "" ||
        query.durationMin !== "" ||
        query.durationMax !== "" ||
        query.fileCountMin !== "" ||
        query.fileCountMax !== "" ||
        query.deletionState.length > 0
    );
};

export const getDatasetFileCount = (dataset: DataSetSummaryDTO) => {
    return dataset.fileCount ?? dataset.files?.length ?? 0;
};

export const getDatasetTotalSize = (dataset: DataSetSummaryDTO) => {
    if (typeof dataset.totalSizeBytes === "number") return dataset.totalSizeBytes;
    if (!dataset.files) return null;

    return dataset.files.reduce((sum, file) => sum + (file.size ?? 0), 0);
};

export const getDatasetDurationMs = (dataset: DataSetSummaryDTO) => {
    const begin = dataset.beginStampUTC?.getTime();
    const end = dataset.endStampUTC?.getTime();

    if (begin === undefined || end === undefined) return null;
    const duration = end - begin;
    return Number.isFinite(duration) && duration >= 0 ? duration : null;
};

const dateFromInput = (value: string) => {
    if (value.trim() === "") return null;
    const parsed = new Date(value);
    return Number.isFinite(parsed.getTime()) ? parsed : null;
};

const getDateSortValue = (value?: Date | null) => value?.getTime() ?? Number.NEGATIVE_INFINITY;

const getSortValue = (dataset: DataSetSummaryDTO, key: DatasetSortKey): number | string => {
    if (key === "name") return (dataset.name ?? dataset.slug ?? dataset.id ?? "").toLowerCase();
    if (key === "begin") return getDateSortValue(dataset.beginStampUTC);
    if (key === "end") return getDateSortValue(dataset.endStampUTC);
    if (key === "duration") return getDatasetDurationMs(dataset) ?? Number.NEGATIVE_INFINITY;
    if (key === "created") return getDateSortValue(dataset.createdStampUTC);
    if (key === "fileCount") return getDatasetFileCount(dataset);
    if (key === "totalSizeBytes") return getDatasetTotalSize(dataset) ?? Number.NEGATIVE_INFINITY;
    return dataset.deletionState ?? "";
};

const compareValues = (a: number | string, b: number | string) => {
    if (typeof a === "string" || typeof b === "string") {
        return String(a).localeCompare(String(b));
    }

    return a - b;
};

const passesDateRange = (value: Date | null | undefined, min: Date | null, max: Date | null) => {
    const time = value?.getTime();

    if (min && (time === undefined || time < min.getTime())) return false;
    if (max && (time === undefined || time > max.getTime())) return false;
    return true;
};

const passesNumberRange = (value: number | null, min: number | null, max: number | null) => {
    if (min !== null && (value === null || value < min)) return false;
    if (max !== null && (value === null || value > max)) return false;
    return true;
};

export const filterDatasets = (datasets: DataSetSummaryDTO[], query: DatasetListQuery) => {
    const beginMin = dateFromInput(query.beginMin);
    const beginMax = dateFromInput(query.beginMax);
    const endMin = dateFromInput(query.endMin);
    const endMax = dateFromInput(query.endMax);
    const durationMinMs = getNumberFilter(query.durationMin);
    const durationMaxMs = getNumberFilter(query.durationMax);
    const fileCountMin = getNumberFilter(query.fileCountMin);
    const fileCountMax = getNumberFilter(query.fileCountMax);

    return datasets.filter((dataset) => {
        if (
            query.deletionState.length > 0 &&
            !query.deletionState.includes(dataset.deletionState ?? "")
        ) {
            return false;
        }
        if (!passesDateRange(dataset.beginStampUTC, beginMin, beginMax)) return false;
        if (!passesDateRange(dataset.endStampUTC, endMin, endMax)) return false;
        if (
            !passesNumberRange(
                getDatasetDurationMs(dataset),
                durationMinMs === null ? null : durationMinMs * 1000,
                durationMaxMs === null ? null : durationMaxMs * 1000
            )
        ) {
            return false;
        }
        if (!passesNumberRange(getDatasetFileCount(dataset), fileCountMin, fileCountMax))
            return false;

        return true;
    });
};

export const sortDatasets = (datasets: DataSetSummaryDTO[], query: DatasetListQuery) => {
    const direction = query.dir === "asc" ? 1 : -1;

    return [...datasets].sort((left, right) => {
        const compared = compareValues(
            getSortValue(left, query.sort),
            getSortValue(right, query.sort)
        );
        if (compared !== 0) return compared * direction;

        return (left.name ?? left.id ?? "").localeCompare(right.name ?? right.id ?? "");
    });
};

export const applyDatasetListQuery = (datasets: DataSetSummaryDTO[], query: DatasetListQuery) => {
    return sortDatasets(filterDatasets(datasets, query), query);
};

export const resetDatasetFilters = (query: DatasetListQuery): DatasetListQuery => ({
    ...query,
    beginMin: "",
    beginMax: "",
    endMin: "",
    endMax: "",
    durationMin: "",
    durationMax: "",
    fileCountMin: "",
    fileCountMax: "",
    deletionState: []
});

export const isDeletionState = (value: string): value is DeletionStateDTO => {
    return Object.values(DeletionStateDTO).includes(value as DeletionStateDTO);
};
