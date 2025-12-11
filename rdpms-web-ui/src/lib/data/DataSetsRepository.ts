import {DataSetsApi, type DataSetSummaryDTO, Configuration, type DataSetDetailedDTO} from '$lib/api_client';

export class DataSetsRepository {
    private readonly ready: Promise<void>;
    private api: DataSetsApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        // Kick off async init immediately, but keep constructor synchronous
        this.ready = configPromise
            .then((conf) => {
                this.api = new DataSetsApi(conf);
            })
            .catch((err) => {
                // Prevent unhandled rejections and surface errors later in ensureReady
                console.error('Failed to initialize DataSetsApi:', err);
                throw err;
            });
    }

    private async ensureReady(): Promise<DataSetsApi> {
        if (this.api) return this.api;
        await this.ready; // wait for config -> api creation
        if (!this.api) {
            throw new Error('DataSetsApi failed to initialize.');
        }
        return this.api;
    }

    public async listAll(): Promise<DataSetSummaryDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1DataDatasetsGet();
    }

    public async listByCollection(collectionId: string): Promise<DataSetSummaryDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1DataDatasetsGet({ collectionId });
    }

    public async getById(id: string): Promise<DataSetDetailedDTO> {
        const api = await this.ensureReady();
        return api.apiV1DataDatasetsIdGet({ id });
    }

    public async create(dto: Partial<DataSetSummaryDTO>): Promise<string> {
        const api = await this.ensureReady();
        return api.apiV1DataDatasetsPost({ dataSetSummaryDTO: dto as DataSetSummaryDTO });
    }

    public async createBatch(dtos: Partial<DataSetSummaryDTO>[]): Promise<void> {
        const api = await this.ensureReady();
        return api.apiV1DataDatasetsBatchPost({ dataSetSummaryDTO: dtos as DataSetSummaryDTO[] });
    }
}