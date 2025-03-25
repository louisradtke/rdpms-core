import {Configuration} from "$lib/api_client";

export async function getApiConfig(): Promise<Configuration> {
    await window.getConfig();

    if (!window.API_BASE_URL) {
        throw new Error('API_BASE_URL is not yet defined. Make sure the runtime configuration is loaded.');
    }

    const conf = new Configuration({
        basePath: window.API_BASE_URL // Assume Configuration expects basePath or similar
    });

    console.log('using config:');
    console.log(conf);

    return conf;
}