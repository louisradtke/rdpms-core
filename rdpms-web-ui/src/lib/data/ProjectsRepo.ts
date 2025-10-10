import {
    type ApiV1ProjectsGetRequest,
    Configuration,
    ProjectsApi,
    type ProjectSummaryDTO,
} from '$lib/api_client';

export class ProjectsRepository {
    private readonly ready: Promise<void>;
    private api: ProjectsApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        // Kick off async init immediately, but keep constructor synchronous
        this.ready = configPromise
            .then((conf) => {
                this.api = new ProjectsApi(conf);
            }).catch((err) => {
                // Prevent unhandled rejections and surface errors later in ensureReady
                console.error('Failed to initialize CollectionsApi:', err);
                throw err;
            });
    }

    private async ensureReady(): Promise<ProjectsApi> {
        if (this.api) return this.api;
        await this.ready; // wait for config -> api creation
        if (!this.api) {
            throw new Error('CollectionsApi failed to initialize.');
        }
        return this.api;
    }

    public async getProjects(requestParameters?: ApiV1ProjectsGetRequest): Promise<ProjectSummaryDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1ProjectsGet(requestParameters);
    }

    public async getProjectById(id: string): Promise<ProjectSummaryDTO> {
        const api = await this.ensureReady();
        return api.apiV1ProjectsIdGet({ id });
    }

    public async updateProject(id: string, dto: Partial<ProjectSummaryDTO>): Promise<void> {
        const api = await this.ensureReady();
        return api.apiV1ProjectsIdPut({ id, projectSummaryDTO: dto as ProjectSummaryDTO });
    }
}