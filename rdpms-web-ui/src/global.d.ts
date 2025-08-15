// global.d.ts
export {};

export interface RuntimeConfig {
    apiBaseUrl: string;

    // Allow additional keys without strict typing; tighten as you formalize the shape.
    [key: string]: unknown;
}

declare global {
    interface Window {
        RUNTIME_CONFIG: RuntimeConfig; // Add the type for your global variable
        getOrFetchConfig(): Promise<RuntimeConfig>;
    }
}