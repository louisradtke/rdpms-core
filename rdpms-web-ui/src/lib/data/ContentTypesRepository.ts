import { ContentTypesApi, type ContentTypeDTO, Configuration } from '$lib/api_client';

export class ContentTypesRepository {
    private readonly ready: Promise<void>;
    private api: ContentTypesApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        this.ready = configPromise
            .then((conf) => {
                this.api = new ContentTypesApi(conf);
            })
            .catch((err) => {
                console.error('Failed to initialize ContentTypesApi:', err);
                throw err;
            });
    }

    private async ensureReady(): Promise<ContentTypesApi> {
        if (this.api) return this.api;
        await this.ready;
        if (!this.api) {
            throw new Error('ContentTypesApi failed to initialize.');
        }
        return this.api;
    }

    public async listAll(): Promise<ContentTypeDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1DataContentTypesGet();
    }

    public async getById(id: string): Promise<ContentTypeDTO> {
        const api = await this.ensureReady();
        return api.apiV1DataContentTypesIdGet({ id });
    }

    public async create(dto: Partial<ContentTypeDTO>): Promise<void> {
        const api = await this.ensureReady();
        return api.apiV1DataContentTypesPost({ contentTypeDTO: dto as ContentTypeDTO });
    }

    public async createBatch(dtos: Partial<ContentTypeDTO>[]): Promise<void> {
        const api = await this.ensureReady();
        return api.apiV1DataContentTypesBatchPost({ contentTypeDTO: dtos as ContentTypeDTO[] });
    }
}
