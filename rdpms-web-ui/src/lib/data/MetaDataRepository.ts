import { Configuration, DataSetsApi, FilesApi, MetaDataApi, type MetaDateDTO, type SchemaValidationResultDTO } from '$lib/api_client';

export class MetaDataRepository {
    private readonly ready: Promise<void>;
    private metadataApi: MetaDataApi | null = null;
    private datasetsApi: DataSetsApi | null = null;
    private filesApi: FilesApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        this.ready = configPromise
            .then((conf) => {
                this.metadataApi = new MetaDataApi(conf);
                this.datasetsApi = new DataSetsApi(conf);
                this.filesApi = new FilesApi(conf);
            })
            .catch((err) => {
                console.error('Failed to initialize MetaDataRepository APIs:', err);
                throw err;
            });
    }

    private async ensureReady(): Promise<{ metadataApi: MetaDataApi; datasetsApi: DataSetsApi; filesApi: FilesApi }> {
        if (this.metadataApi && this.datasetsApi && this.filesApi) {
            return { metadataApi: this.metadataApi, datasetsApi: this.datasetsApi, filesApi: this.filesApi };
        }

        await this.ready;

        if (!this.metadataApi || !this.datasetsApi || !this.filesApi) {
            throw new Error('MetaDataRepository APIs failed to initialize.');
        }

        return { metadataApi: this.metadataApi, datasetsApi: this.datasetsApi, filesApi: this.filesApi };
    }

    public async getById(id: string): Promise<MetaDateDTO> {
        const { metadataApi } = await this.ensureReady();
        return metadataApi.apiV1DataMetadataIdGet({ id });
    }

    public async getJsonValueByFileId(fileId: string): Promise<unknown> {
        const { filesApi } = await this.ensureReady();
        const blob = await filesApi.apiV1DataFilesIdBlobGet({ id: fileId });
        const text = await blob.text();
        return JSON.parse(text);
    }

    public async setDatasetMetadata(datasetId: string, key: string, jsonValue: string): Promise<MetaDateDTO> {
        const { datasetsApi } = await this.ensureReady();
        return datasetsApi.apiV1DataDatasetsIdMetadataKeyPut({
            id: datasetId,
            key,
            body: jsonValue
        });
    }

    public async setFileMetadata(fileId: string, key: string, jsonValue: string): Promise<MetaDateDTO> {
        const { filesApi } = await this.ensureReady();
        return filesApi.apiV1DataFilesIdMetadataKeyPut({
            id: fileId,
            key,
            body: jsonValue
        });
    }

    public async validateMetadata(
        metadataId: string,
        schemaId: string,
        verbose = true
    ): Promise<SchemaValidationResultDTO> {
        const { metadataApi } = await this.ensureReady();
        return metadataApi.apiV1DataMetadataIdValidateSchemaIdPut({
            id: metadataId,
            schemaId,
            verbose
        });
    }
}
