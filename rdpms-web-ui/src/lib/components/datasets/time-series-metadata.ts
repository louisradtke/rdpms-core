export type TopicMetadata = {
    messageCount?: number;
    firstMessageTimestamp?: string;
    lastMessageTimestamp?: string;
};

export type FieldType = {
    descriptor?: string;
    vocabularyReference?: string;
    type?: string;
    format?: string;
};

export type FieldDefinition = {
    name?: string;
    type?: FieldType | MessageType;
};

export type MessageType = {
    name?: string;
    description?: string;
    reference?: string;
    fields?: FieldDefinition[];
};

export type TimeSeriesTopic = {
    name?: string;
    metadata?: TopicMetadata;
    messageType?: MessageType;
};

export type TimeSeriesContainer = {
    topics?: TimeSeriesTopic[];
};

export type TopicFolderNode = {
    kind: "folder";
    name: string;
    path: string;
    key: string;
    children: TopicTreeNode[];
    topicCount: number;
    types: string[];
};

export type TopicLeafNode = {
    kind: "topic";
    name: string;
    path: string;
    key: string;
    topic: TimeSeriesTopic;
};

export type TopicTreeNode = TopicFolderNode | TopicLeafNode;

type InternalFolder = {
    folders: Map<string, InternalFolder>;
    topics: TopicLeafNode[];
};

export type TopicStats = {
    topicCount: number;
    typedTopicCount: number;
    totalMessageCount: number | null;
    begin: Date | null;
    end: Date | null;
    durationSeconds: number | null;
    typeCount: number;
};

const isObject = (value: unknown): value is Record<string, unknown> =>
    typeof value === "object" && value !== null && !Array.isArray(value);

const asString = (value: unknown): string | undefined =>
    typeof value === "string" ? value : undefined;

const asNumber = (value: unknown): number | undefined =>
    typeof value === "number" && Number.isFinite(value) ? value : undefined;

const parseMetadata = (value: unknown): TopicMetadata => {
    if (!isObject(value)) return {};
    return {
        messageCount: asNumber(value.messageCount),
        firstMessageTimestamp: asString(value.firstMessageTimestamp),
        lastMessageTimestamp: asString(value.lastMessageTimestamp)
    };
};

const parseMessageType = (value: unknown): MessageType => {
    if (!isObject(value)) return {};
    return {
        name: asString(value.name),
        description: asString(value.description),
        reference: asString(value.reference),
        fields: Array.isArray(value.fields) ? (value.fields as FieldDefinition[]) : []
    };
};

export const parseTimeSeriesContainer = (value: unknown): TimeSeriesContainer | null => {
    if (!isObject(value) || !Array.isArray(value.topics)) return null;

    return {
        topics: value.topics.filter(isObject).map((topic) => ({
            name: asString(topic.name),
            metadata: parseMetadata(topic.metadata),
            messageType: parseMessageType(topic.messageType)
        }))
    };
};

export const normalizeTopicName = (topic: TimeSeriesTopic): string =>
    topic.name?.trim() || "unnamed";

export const typeBasename = (messageType?: MessageType): string => {
    const name = messageType?.name?.trim();
    if (!name) return "unknown";
    return name.split("/").filter(Boolean).at(-1) ?? name;
};

export const parseDate = (value?: string): Date | null => {
    if (!value) return null;
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
};

export const formatDateTime = (date: Date | null): string => {
    if (!date) return "unknown";
    return new Intl.DateTimeFormat(undefined, {
        dateStyle: "medium",
        timeStyle: "medium"
    }).format(date);
};

export const formatDuration = (seconds: number | null): string => {
    if (seconds === null || !Number.isFinite(seconds) || seconds < 0) return "unknown";
    if (seconds < 1) return "<1s";

    const rounded = Math.round(seconds);
    const hours = Math.floor(rounded / 3600);
    const minutes = Math.floor((rounded % 3600) / 60);
    const secs = rounded % 60;

    if (hours > 0) return `${hours}h ${minutes}m`;
    if (minutes > 0) return `${minutes}m ${secs}s`;
    return `${secs}s`;
};

export const formatCount = (value: number | null | undefined): string => {
    if (value === null || value === undefined) return "unknown";
    return new Intl.NumberFormat().format(value);
};

export const formatRate = (topic: TimeSeriesTopic): string => {
    const count = topic.metadata?.messageCount;
    const first = parseDate(topic.metadata?.firstMessageTimestamp);
    const last = parseDate(topic.metadata?.lastMessageTimestamp);
    if (count === undefined || !first || !last) return "unknown";

    const seconds = (last.getTime() - first.getTime()) / 1000;
    if (seconds <= 0) return "unknown";

    const rate = count / seconds;
    if (rate >= 10) return `${rate.toFixed(1)} Hz`;
    if (rate >= 1) return `${rate.toFixed(2)} Hz`;
    return `${rate.toFixed(3)} Hz`;
};

export const topicDurationSeconds = (topic: TimeSeriesTopic): number | null => {
    const first = parseDate(topic.metadata?.firstMessageTimestamp);
    const last = parseDate(topic.metadata?.lastMessageTimestamp);
    if (!first || !last) return null;
    return Math.max(0, (last.getTime() - first.getTime()) / 1000);
};

const normalizeSegments = (topic: TimeSeriesTopic): string[] =>
    normalizeTopicName(topic)
        .replace(/\\/g, "/")
        .split("/")
        .map((segment) => segment.trim())
        .filter((segment) => segment.length > 0);

const makeTopicKey = (path: string, topic: TimeSeriesTopic, index: number): string => {
    const messageType = topic.messageType?.name ?? "unknown";
    const first = topic.metadata?.firstMessageTimestamp ?? "";
    const last = topic.metadata?.lastMessageTimestamp ?? "";
    const count = topic.metadata?.messageCount ?? "";
    return `${path}::${messageType}::${first}::${last}::${count}::${index}`;
};

const toPublicNodes = (folder: InternalFolder, pathPrefix = ""): TopicTreeNode[] => {
    const children: TopicTreeNode[] = [];

    for (const [name, child] of Array.from(folder.folders.entries()).sort(([left], [right]) =>
        left.localeCompare(right)
    )) {
        const path = pathPrefix ? `${pathPrefix}/${name}` : `/${name}`;
        const childNodes = toPublicNodes(child, path);
        const leafs = collectTopics(childNodes);
        children.push({
            kind: "folder",
            name,
            path,
            key: `folder:${path}`,
            children: childNodes,
            topicCount: leafs.length,
            types: Array.from(new Set(leafs.map((topic) => typeBasename(topic.messageType)))).sort()
        });
    }

    children.push(
        ...folder.topics
            .sort((left, right) => left.name.localeCompare(right.name))
            .map((topic, index) => ({
                ...topic,
                key: makeTopicKey(topic.path, topic.topic, index)
            }))
    );

    return children;
};

export const buildTopicTree = (topics: TimeSeriesTopic[]): TopicTreeNode[] => {
    const root: InternalFolder = { folders: new Map(), topics: [] };

    for (const topic of topics) {
        const segments = normalizeSegments(topic);
        if (segments.length === 0) {
            root.topics.push({
                kind: "topic",
                name: normalizeTopicName(topic),
                path: normalizeTopicName(topic),
                key: "",
                topic
            });
            continue;
        }

        let current = root;
        let path = "";
        for (const segment of segments.slice(0, -1)) {
            path = path ? `${path}/${segment}` : `/${segment}`;
            if (!current.folders.has(segment)) {
                current.folders.set(segment, { folders: new Map(), topics: [] });
            }
            current = current.folders.get(segment)!;
        }

        const leafName = segments.at(-1) ?? normalizeTopicName(topic);
        const leafPath = path ? `${path}/${leafName}` : `/${leafName}`;
        current.topics.push({
            kind: "topic",
            name: leafName,
            path: leafPath,
            key: "",
            topic
        });
    }

    return toPublicNodes(root);
};

export const collectTopics = (nodes: TopicTreeNode[]): TimeSeriesTopic[] =>
    nodes.flatMap((node) => (node.kind === "topic" ? [node.topic] : collectTopics(node.children)));

export const calculateTopicStats = (topics: TimeSeriesTopic[]): TopicStats => {
    let totalMessageCount = 0;
    let allHaveMessageCount = topics.length > 0;
    let begin: Date | null = null;
    let end: Date | null = null;
    const types = new Set<string>();

    for (const topic of topics) {
        const count = topic.metadata?.messageCount;
        if (count === undefined) {
            allHaveMessageCount = false;
        } else {
            totalMessageCount += count;
        }

        const first = parseDate(topic.metadata?.firstMessageTimestamp);
        const last = parseDate(topic.metadata?.lastMessageTimestamp);
        if (first && (!begin || first < begin)) begin = first;
        if (last && (!end || last > end)) end = last;

        if (topic.messageType?.name) {
            types.add(topic.messageType.name);
        }
    }

    return {
        topicCount: topics.length,
        typedTopicCount: topics.filter((topic) => Boolean(topic.messageType?.name)).length,
        totalMessageCount: allHaveMessageCount ? totalMessageCount : null,
        begin,
        end,
        durationSeconds:
            begin && end ? Math.max(0, (end.getTime() - begin.getTime()) / 1000) : null,
        typeCount: types.size
    };
};
