/**
 * Runtime configuration loader (TypeScript)
 *
 * Exposes:
 * - window.getOrFetchConfig(): Promise<RuntimeConfig>  -- returns the runtime config (cached after first successful fetch)
 *
 * Behavior:
 * - If config already loaded, getOrFetchConfig resolves immediately with it.
 * - If a fetch is in progress, callers await the same promise (no concurrent fetches).
 * - On first successful fetch, a simple runtime validation is performed.
 */
import type {RuntimeConfig} from "../../global";
import {Configuration} from "$lib/api_client";

let _runtimeConfig: RuntimeConfig | null = null; // cached config object after successful fetch
let _fetchPromise: Promise<RuntimeConfig> | null = null; // promise for in-flight fetch to avoid duplicate fetches

function validateConfig(cfg: unknown): asserts cfg is RuntimeConfig {
    if (!cfg || typeof cfg !== 'object') {
        throw new Error('Configuration must be an object.');
    }
    const obj = cfg as Record<string, unknown>;
    if (typeof obj.apiBaseUrl !== 'string') {
        throw new Error('Configuration is missing "apiBaseUrl" string property.');
    }
}

async function fetchConfigFromServer(): Promise<RuntimeConfig> {
    const response = await fetch('/config.json'); // optional: { cache: 'no-store' }
    if (!response.ok) {
        throw new Error(`Could not load configuration. HTTP ${response.status}`);
    }
    const cfg = (await response.json()) as unknown;
    validateConfig(cfg);
    return cfg;
}

/**
 * Returns the runtime config. If not loaded yet, fetch it exactly once.
 */
export async function getOrFetchConfig(): Promise<RuntimeConfig> {
    if (_runtimeConfig) {
        return _runtimeConfig;
    }

    if (_fetchPromise) {
        return _fetchPromise;
    }

    _fetchPromise = (async () => {
        try {
            // set globals / module cache
            _runtimeConfig = await fetchConfigFromServer();

            // Optional: log once on success
            // console.info('Runtime config loaded:', _runtimeConfig);

            return _runtimeConfig;
        } finally {
            // clear in-flight marker (keep _runtimeConfig if set)
            _fetchPromise = null;
        }
    })();

    return _fetchPromise;
}

export function toApiConfig(runtimeConfig: RuntimeConfig): Configuration {
    return new Configuration({
        basePath: runtimeConfig.apiBaseUrl
    });
}