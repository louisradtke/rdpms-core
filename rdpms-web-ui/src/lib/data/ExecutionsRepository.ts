import {
    ExecutionsApi,
    type ExecutionSummaryDTO,
    Configuration
} from "$lib/api_client";

export class ExecutionsRepository {
    private readonly ready: Promise<void>;
    private api: ExecutionsApi | null = null;

    constructor(configPromise: Promise<Configuration>) {
        this.ready = configPromise
            .then((conf) => {
                this.api = new ExecutionsApi(conf);
            })
            .catch((err) => {
                console.error("Failed to initialize ExecutionsApi:", err);
                throw err;
            });
    }

    private async ensureReady(): Promise<ExecutionsApi> {
        if (this.api) return this.api;
        await this.ready;
        if (!this.api) {
            throw new Error("ExecutionsApi failed to initialize.");
        }
        return this.api;
    }

    public async listByAncestor(datasetId: string): Promise<ExecutionSummaryDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1ExecutionsGet({ ancestorOf: datasetId });
    }

    public async listByChild(datasetId: string): Promise<ExecutionSummaryDTO[]> {
        const api = await this.ensureReady();
        return api.apiV1ExecutionsGet({ childOf: datasetId });
    }
}
