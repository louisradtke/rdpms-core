<script lang="ts">
    import LoadingCircle from "$lib/layout/LoadingCircle.svelte";
    import type { CorePluginPolicy } from "$lib/layout/displayPlugins/plugin-registry";
    import type { Action } from "svelte/action";
    import "leaflet/dist/leaflet.css";

    let {
        dataUri,
        policy,
        options
    }: {
        dataUri: string;
        policy?: CorePluginPolicy;
        options?: Record<string, unknown>;
    } = $props();

    interface TrackRendererOptions {
        timeField?: string;
        latField?: string;
        lonField?: string;
        zoom?: number;
    }

    interface TrackPoint {
        stamp: string;
        lat: number;
        lon: number;
        timestampMs: number;
    }

    interface ParsedMapData {
        points: TrackPoint[];
        totalDistanceKm: number;
        minLat: number;
        maxLat: number;
        minLon: number;
        maxLon: number;
        centerLat: number;
        centerLon: number;
    }

    const DEFAULT_MAX_SIZE = 25 * 1024 * 1024;
    const DEFAULT_ZOOM = 14;

    const parsedOptions = $derived((options ?? {}) as TrackRendererOptions);
    const maxSize = $derived(policy?.display?.maxBytes ?? DEFAULT_MAX_SIZE);
    const oversizeMessage = $derived(
        policy?.display?.oversizeMessage ?? "File too large for OpenStreetMap preview (max 25 MiB)"
    );

    function splitCsvLine(line: string): string[] {
        const result: string[] = [];
        let current = "";
        let inQuotes = false;
        for (let i = 0; i < line.length; i += 1) {
            const ch = line[i];
            if (ch === '"') {
                if (inQuotes && line[i + 1] === '"') {
                    current += '"';
                    i += 1;
                } else {
                    inQuotes = !inQuotes;
                }
                continue;
            }
            if (ch === "," && !inQuotes) {
                result.push(current.trim());
                current = "";
                continue;
            }
            current += ch;
        }
        result.push(current.trim());
        return result;
    }

    const normalize = (value: string): string => value.trim().toLowerCase();

    function haversineKm(a: TrackPoint, b: TrackPoint): number {
        const earthRadiusKm = 6371;
        const toRad = (deg: number) => (deg * Math.PI) / 180;
        const lat1 = toRad(a.lat);
        const lat2 = toRad(b.lat);
        const dLat = toRad(b.lat - a.lat);
        const dLon = toRad(b.lon - a.lon);
        const q =
            Math.sin(dLat / 2) ** 2 + Math.cos(lat1) * Math.cos(lat2) * Math.sin(dLon / 2) ** 2;
        const c = 2 * Math.atan2(Math.sqrt(q), Math.sqrt(1 - q));
        return earthRadiusKm * c;
    }

    async function fetchTrackData(uri: string): Promise<ParsedMapData> {
        if (!uri) {
            throw new Error("No URI available");
        }

        const response = await fetch(uri);
        if (!response.ok) {
            throw new Error(`Failed to fetch file: ${response.status} ${response.statusText}`);
        }

        const contentLength = response.headers.get("content-length");
        if (contentLength && Number.parseInt(contentLength, 10) > maxSize) {
            throw new Error(oversizeMessage);
        }

        const text = await response.text();
        const actualSize = new Blob([text]).size;
        if (actualSize > maxSize) {
            throw new Error(oversizeMessage);
        }

        const lines = text
            .split(/\r?\n/)
            .map((line) => line.trim())
            .filter((line) => line.length > 0);
        if (lines.length < 2) {
            throw new Error("CSV must contain a header and at least one row");
        }

        const header = splitCsvLine(lines[0]).map((h) => h.replace(/^"|"$/g, ""));
        const timeFieldName = normalize(parsedOptions.timeField ?? "stamp");
        const latFieldName = normalize(parsedOptions.latField ?? "lat");
        const lonFieldName = normalize(parsedOptions.lonField ?? "lon");

        const stampIndex = header.findIndex((h) => normalize(h) === timeFieldName);
        const latIndex = header.findIndex((h) => normalize(h) === latFieldName);
        const lonIndex = header.findIndex((h) => normalize(h) === lonFieldName);
        if (stampIndex < 0 || latIndex < 0 || lonIndex < 0) {
            throw new Error(
                `CSV must contain columns: ${timeFieldName}, ${latFieldName}, ${lonFieldName}`
            );
        }

        const points: TrackPoint[] = [];
        for (const line of lines.slice(1)) {
            const fields = splitCsvLine(line).map((field) => field.replace(/^"|"$/g, ""));
            const stamp = fields[stampIndex] ?? "";
            const lat = Number.parseFloat(fields[latIndex] ?? "");
            const lon = Number.parseFloat(fields[lonIndex] ?? "");
            const timestampMs = Date.parse(stamp);
            const isValid =
                Number.isFinite(timestampMs) &&
                Number.isFinite(lat) &&
                Number.isFinite(lon) &&
                lat >= -90 &&
                lat <= 90 &&
                lon >= -180 &&
                lon <= 180;
            if (isValid) {
                points.push({ stamp, lat, lon, timestampMs });
            }
        }

        if (points.length === 0) {
            throw new Error("No valid rows found in CSV (expected ISO stamp with tz offset, lat, lon)");
        }

        points.sort((a, b) => a.timestampMs - b.timestampMs);

        let minLat = Number.POSITIVE_INFINITY;
        let maxLat = Number.NEGATIVE_INFINITY;
        let minLon = Number.POSITIVE_INFINITY;
        let maxLon = Number.NEGATIVE_INFINITY;
        let totalDistanceKm = 0;

        for (let i = 0; i < points.length; i += 1) {
            const point = points[i];
            minLat = Math.min(minLat, point.lat);
            maxLat = Math.max(maxLat, point.lat);
            minLon = Math.min(minLon, point.lon);
            maxLon = Math.max(maxLon, point.lon);
            if (i > 0) {
                totalDistanceKm += haversineKm(points[i - 1], point);
            }
        }

        return {
            points,
            totalDistanceKm,
            minLat,
            maxLat,
            minLon,
            maxLon,
            centerLat: (minLat + maxLat) / 2,
            centerLon: (minLon + maxLon) / 2
        };
    }

    const trackPromise = $derived(fetchTrackData(dataUri));

    type LeafletActionParams = {
        track: ParsedMapData;
        zoom?: number;
    };

    const leafletMapAction: Action<HTMLDivElement, LeafletActionParams> = (node, params) => {
        let cancelled = false;
        let map: import("leaflet").Map | null = null;
        let lineLayer: import("leaflet").Polyline | null = null;
        let startMarker: import("leaflet").CircleMarker | null = null;
        let endMarker: import("leaflet").CircleMarker | null = null;
        let tileLayer: import("leaflet").TileLayer | null = null;

        const init = async (nextParams: LeafletActionParams) => {
            const L = await import("leaflet");
            if (cancelled) return;

            if (!map) {
                map = L.map(node, {
                    zoomControl: true,
                    attributionControl: true
                });
                tileLayer = L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
                    maxZoom: 19,
                    attribution:
                        '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                }).addTo(map);
            }

            lineLayer?.remove();
            startMarker?.remove();
            endMarker?.remove();

            const latLngs = nextParams.track.points.map((point) => [point.lat, point.lon] as [number, number]);
            lineLayer = L.polyline(latLngs, {
                color: "#dc2626",
                weight: 3
            }).addTo(map);

            const first = nextParams.track.points[0];
            const last = nextParams.track.points[nextParams.track.points.length - 1];
            startMarker = L.circleMarker([first.lat, first.lon], {
                radius: 5,
                color: "#16a34a",
                fillColor: "#16a34a",
                fillOpacity: 1
            }).addTo(map);
            endMarker = L.circleMarker([last.lat, last.lon], {
                radius: 5,
                color: "#dc2626",
                fillColor: "#dc2626",
                fillOpacity: 1
            }).addTo(map);

            const explicitZoom = nextParams.zoom;
            if (typeof explicitZoom === "number" && Number.isFinite(explicitZoom)) {
                map.setView([nextParams.track.centerLat, nextParams.track.centerLon], explicitZoom);
            } else {
                map.fitBounds(lineLayer.getBounds(), { padding: [24, 24], maxZoom: 18 });
            }

            requestAnimationFrame(() => {
                map?.invalidateSize();
            });
        };

        void init(params);

        const resizeObserver = new ResizeObserver(() => {
            map?.invalidateSize();
        });
        resizeObserver.observe(node);

        return {
            update(nextParams) {
                void init(nextParams);
            },
            destroy() {
                cancelled = true;
                resizeObserver.disconnect();
                map?.remove();
                map = null;
            }
        };
    };
</script>

{#await trackPromise}
    <div class="flex justify-center items-center">
        <LoadingCircle />
    </div>
{:then track}
    <div class="space-y-3">
        <div class="grid grid-cols-1 gap-2 text-xs text-gray-600 sm:grid-cols-3 lg:grid-cols-6">
            <div><span class="font-medium text-gray-800">Points:</span> {track.points.length}</div>
            <div>
                <span class="font-medium text-gray-800">Distance:</span>
                {track.totalDistanceKm.toFixed(3)} km
            </div>
            <div>
                <span class="font-medium text-gray-800">Center:</span>
                {track.centerLat.toFixed(5)}, {track.centerLon.toFixed(5)}
            </div>
            <div><span class="font-medium text-gray-800">Start:</span> {track.points[0].stamp}</div>
            <div>
                <span class="font-medium text-gray-800">End:</span>
                {track.points[track.points.length - 1].stamp}
            </div>
            <div>
                <a
                    class="font-medium text-blue-700 hover:text-blue-900"
                    href={`https://www.openstreetmap.org/?mlat=${track.centerLat}&mlon=${track.centerLon}#map=${parsedOptions.zoom ?? DEFAULT_ZOOM}/${track.centerLat}/${track.centerLon}`}
                    target="_blank"
                    rel="noreferrer"
                    >Open in OSM</a
                >
            </div>
        </div>

        <div class="rounded border border-gray-200 bg-white p-2">
            <div
                class="h-[28rem] w-full rounded"
                use:leafletMapAction={{ track, zoom: parsedOptions.zoom }}
            ></div>
        </div>

        <div class="text-xs text-gray-500">
            lat [{track.minLat.toFixed(6)}, {track.maxLat.toFixed(6)}], lon [{track.minLon.toFixed(
                6
            )}, {track.maxLon.toFixed(6)}]
        </div>
    </div>
{:catch error}
    <div class="rounded border border-red-200 bg-red-50 p-2 text-sm text-red-800">
        {error?.message ?? "Failed to load OpenStreetMap preview"}
    </div>
{/await}
