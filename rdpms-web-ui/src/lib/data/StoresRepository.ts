import { StoresApi, type DataStoreSummaryDTO, Configuration } from '$lib/api_client';

export class StoresRepository {
    private readonly ready: Promise<void>;
    private api: StoresApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        this.ready = configPromise
            .then((conf) => {
                this.api = new StoresApi(conf);
            })
            .catch((err) => {
                console.error('Failed to initialize StoresApi:', err);
                throw err;
            });
    }

    private async ensureReady(): Promise<StoresApi> {
        if (this.api) return this.api;
        await this.ready;
        if (!this.api) {
            throw new Error('StoresApi failed to initialize.');
        }
        return this.api;
    }

    public async listAll(): Promise<DataStoreSummaryDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1DataStoresGet();
    }

    public async getById(id: string): Promise<DataStoreSummaryDTO> {
        const api = await this.ensureReady();
        return api.apiV1DataStoresIdGet({ id });
    }
}
