/**
 * File that fetches a config from a server
 */

/**
 * Essentially the prefix for API requests
 * @type {string}
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
let API_BASE_URL = '';

async function getConfig() {
    const response = await fetch('/config.json'); // Fetch runtime config
    if (!response.ok) {
        throw new Error('Could not load configuration.');
    }
    return response.json();
}

getConfig().then((config) => {
    API_BASE_URL = config['apiBaseUrl'] ?? '';
    console.log('config loaded/fetched from server');
});
