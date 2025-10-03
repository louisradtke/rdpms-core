import { FilesApi, type FileSummaryDTO, type FileCreateRequestDTO, type FileCreateResponseDTO, Configuration } from '$lib/api_client';

export class FilesRepository {
    private readonly ready: Promise<void>;
    private api: FilesApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        this.ready = configPromise
            .then((conf) => {
                this.api = new FilesApi(conf);
            })
            .catch((err) => {
                console.error('Failed to initialize FilesApi:', err);
                throw err;
            });
    }

    private async ensureReady(): Promise<FilesApi> {
        if (this.api) return this.api;
        await this.ready;
        if (!this.api) {
            throw new Error('FilesApi failed to initialize.');
        }
        return this.api;
    }

    public async listAll(): Promise<FileSummaryDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1DataFilesGet();
    }

    public async getById(id: string): Promise<FileSummaryDTO> {
        const api = await this.ensureReady();
        return api.apiV1DataFilesIdGet({ id });
    }

    public async create(request: FileCreateRequestDTO): Promise<FileCreateResponseDTO> {
        const api = await this.ensureReady();
        return api.apiV1DataFilesPost({ fileCreateRequestDTO: request });
    }
}
