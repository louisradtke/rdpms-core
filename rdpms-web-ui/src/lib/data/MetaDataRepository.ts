import { Configuration, FilesApi, MetaDataApi, type MetaDateDTO } from '$lib/api_client';

export class MetaDataRepository {
    private readonly ready: Promise<void>;
    private metadataApi: MetaDataApi | null = null;
    private filesApi: FilesApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        this.ready = configPromise
            .then((conf) => {
                this.metadataApi = new MetaDataApi(conf);
                this.filesApi = new FilesApi(conf);
            })
            .catch((err) => {
                console.error('Failed to initialize MetaDataRepository APIs:', err);
                throw err;
            });
    }

    private async ensureReady(): Promise<{ metadataApi: MetaDataApi; filesApi: FilesApi }> {
        if (this.metadataApi && this.filesApi) {
            return { metadataApi: this.metadataApi, filesApi: this.filesApi };
        }

        await this.ready;

        if (!this.metadataApi || !this.filesApi) {
            throw new Error('MetaDataRepository APIs failed to initialize.');
        }

        return { metadataApi: this.metadataApi, filesApi: this.filesApi };
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
}
