// global.d.ts
export {};

declare global {
    interface Window {
        API_BASE_URL: string; // Add the type for your global variable
        getConfig(): Promise<unknown>;
    }
}