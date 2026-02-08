import { Configuration, MetaDataApi, type SchemaDTO } from '$lib/api_client';

export class SchemasRepository {
    private readonly ready: Promise<void>;
    private api: MetaDataApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        this.ready = configPromise
            .then((conf) => {
                this.api = new MetaDataApi(conf);
            })
            .catch((err) => {
                console.error('Failed to initialize SchemasRepository:', err);
                throw err;
            });
    }

    private async ensureReady(): Promise<MetaDataApi> {
        if (this.api) return this.api;
        await this.ready;
        if (!this.api) {
            throw new Error('SchemasRepository API failed to initialize.');
        }
        return this.api;
    }

    public async listSchemas(): Promise<SchemaDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1DataSchemasGet();
    }

    public async addSchema(schemaJson: string, _schemaId?: string): Promise<void> {
        const parsed = JSON.parse(schemaJson);
        const api = await this.ensureReady();
        await api.apiV1DataSchemasPost({
            body: parsed
        });
    }

    public async getSchemaRaw(schemaDbId: string): Promise<string> {
        const api = await this.ensureReady();
        const blob = await api.apiV1DataSchemasIdBlobGet({ id: schemaDbId });
        return await blob.text();
    }
}
