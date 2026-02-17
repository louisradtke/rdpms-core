import {
    type ApiV1DataCollectionsGetRequest, type CollectionDetailedDTO,
    CollectionsApi,
    type CollectionSummaryDTO,
    Configuration,
    type MetadataColumnTargetDTO
} from '$lib/api_client';
import {isGuid} from "$lib/util/url-helper";

export class CollectionsRepository {
    private readonly ready: Promise<void>;
    private api: CollectionsApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        // Kick off async init immediately, but keep constructor synchronous
        this.ready = configPromise
            .then((conf) => {
                this.api = new CollectionsApi(conf);
            }).
            catch((err) => {
                // Prevent unhandled rejections and surface errors later in ensureReady
                console.error('Failed to initialize CollectionsApi:', err);
                throw err;
            });
    }

    private async ensureReady(): Promise<CollectionsApi> {
        if (this.api) return this.api;
        await this.ready; // wait for config -> api creation
        if (!this.api) {
            throw new Error('CollectionsApi failed to initialize.');
        }
        return this.api;
    }

    public async getCollections(requestParameters?: ApiV1DataCollectionsGetRequest): Promise<CollectionSummaryDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1DataCollectionsGet(requestParameters);
    }

    public async getCollectionById(id: string): Promise<CollectionDetailedDTO> {
        const api = await this.ensureReady()
        return api.apiV1DataCollectionsIdGet({ id });
    }

    /**
     * Returns a collection by its ID or slug.
     * @param idOrSlug The collection ID or slug.
     * @param projectIdOrSlug
     * @returns The collection.
     */
    public async getCollectionByIdOrSlug(idOrSlug: string, projectIdOrSlug: string | undefined)
        : Promise<CollectionDetailedDTO> {
        if (isGuid(idOrSlug)) {
            return this.getCollectionById(idOrSlug)
        }

        const args: ApiV1DataCollectionsGetRequest = { slug: idOrSlug };
        if (projectIdOrSlug) {
            if (isGuid(idOrSlug)) args.projectId = projectIdOrSlug;
            else args.projectSlug = projectIdOrSlug;
        }

        return this.getCollections(args).then((list) => {
            if (!list?.length) throw new Error('Collection not found');
            return list[0];
        });
    }

    public async createCollection(dto: Partial<CollectionSummaryDTO>): Promise<void> {
        const api = await this.ensureReady();
        return api.apiV1DataCollectionsPost({ collectionSummaryDTO: dto as CollectionSummaryDTO });
    }

    public async upsertMetadataColumn(
        collectionId: string,
        key: string,
        options: { schemaId: string; defaultMetadataId?: string; target?: MetadataColumnTargetDTO }
    ): Promise<void> {
        const api = await this.ensureReady();
        return api.apiV1DataCollectionsIdMetadataKeyPut({
            id: collectionId,
            key,
            schemaId: options.schemaId,
            defaultMetadataId: options?.defaultMetadataId,
            target: options?.target,
        });
    }

    public async renameMetadataColumn(
        collectionId: string,
        key: string,
        newKey: string,
        target?: MetadataColumnTargetDTO
    ): Promise<void> {
        const api = await this.ensureReady();
        return api.apiV1DataCollectionsIdMetadataKeyPost({
            id: collectionId,
            key,
            newKey,
            target
        });
    }

    public async deleteMetadataColumn(
        collectionId: string,
        key: string,
        target?: MetadataColumnTargetDTO
    ): Promise<void> {
        const api = await this.ensureReady();
        return api.apiV1DataCollectionsIdMetadataKeyDelete({
            id: collectionId,
            key,
            target
        });
    }
}
