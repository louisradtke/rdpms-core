// TypeScript
import { CollectionsApi, type CollectionSummaryDTO, Configuration } from '$lib/api_client';

export class CollectionsRepository {
    private readonly ready: Promise<void>;
    private api: CollectionsApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        // Kick off async init immediately, but keep constructor synchronous
        this.ready = configPromise.then((conf) => {
            this.api = new CollectionsApi(conf);
        }).catch((err) => {
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

    public async getCollections(): Promise<CollectionSummaryDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1DataCollectionsGet();
    }

    public async createCollection(dto: Partial<CollectionSummaryDTO>): Promise<void> {
        const api = await this.ensureReady();
        return api.apiV1DataCollectionsPost({ collectionSummaryDTO: dto as CollectionSummaryDTO });
    }
}