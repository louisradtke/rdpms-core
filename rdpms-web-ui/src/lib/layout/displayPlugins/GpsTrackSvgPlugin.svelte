<script lang="ts">
    import LoadingCircle from '$lib/layout/LoadingCircle.svelte';

    let { dataUri }: { dataUri: string } = $props();

    const MAX_SIZE = 2 * 1024 * 1024;
    const SVG_WIDTH = 900;
    const SVG_HEIGHT = 320;
    const SVG_PADDING = 24;

    interface TrackPoint {
        stamp: string;
        lat: number;
        lon: number;
        timestampMs: number;
    }

    interface TrackData {
        points: TrackPoint[];
        pathData: string;
        minLat: number;
        maxLat: number;
        minLon: number;
        maxLon: number;
        totalDistanceKm: number;
    }

    function splitCsvLine(line: string): string[] {
        const result: string[] = [];
        let current = '';
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

            if (ch === ',' && !inQuotes) {
                result.push(current.trim());
                current = '';
                continue;
            }

            current += ch;
        }

        result.push(current.trim());
        return result;
    }

    function project(value: number, minValue: number, maxValue: number, outMin: number, outMax: number): number {
        if (maxValue - minValue === 0) {
            return (outMin + outMax) / 2;
        }

        return outMin + ((value - minValue) / (maxValue - minValue)) * (outMax - outMin);
    }

    function haversineKm(a: TrackPoint, b: TrackPoint): number {
        const earthRadiusKm = 6371;
        const toRad = (deg: number) => (deg * Math.PI) / 180;

        const lat1 = toRad(a.lat);
        const lat2 = toRad(b.lat);
        const dLat = toRad(b.lat - a.lat);
        const dLon = toRad(b.lon - a.lon);

        const q = Math.sin(dLat / 2) ** 2
            + Math.cos(lat1) * Math.cos(lat2) * Math.sin(dLon / 2) ** 2;
        const c = 2 * Math.atan2(Math.sqrt(q), Math.sqrt(1 - q));

        return earthRadiusKm * c;
    }

    async function fetchTrackData(uri: string): Promise<TrackData> {
        if (!uri) {
            throw new Error('No URI available');
        }

        const response = await fetch(uri);
        if (!response.ok) {
            throw new Error(`Failed to fetch file: ${response.status} ${response.statusText}`);
        }

        const contentLength = response.headers.get('content-length');
        if (contentLength && Number.parseInt(contentLength, 10) > MAX_SIZE) {
            throw new Error('File too large for GPS preview (max 2 MB)');
        }

        const text = await response.text();
        const actualSize = new Blob([text]).size;
        if (actualSize > MAX_SIZE) {
            throw new Error('File too large for GPS preview (max 2 MB)');
        }

        const lines = text
            .split(/\r?\n/)
            .map((line) => line.trim())
            .filter((line) => line.length > 0);

        if (lines.length < 2) {
            throw new Error('CSV must contain a header and at least one row');
        }

        const header = splitCsvLine(lines[0]).map((h) => h.toLowerCase());
        const stampIndex = header.indexOf('stamp');
        const latIndex = header.indexOf('lat');
        const lonIndex = header.indexOf('lon');

        if (stampIndex < 0 || latIndex < 0 || lonIndex < 0) {
            throw new Error('CSV must contain columns: stamp, lat, lon');
        }

        const points: TrackPoint[] = [];
        for (const line of lines.slice(1)) {
            const fields = splitCsvLine(line);
            const stamp = fields[stampIndex] ?? '';
            const latRaw = fields[latIndex] ?? '';
            const lonRaw = fields[lonIndex] ?? '';

            const timestampMs = Date.parse(stamp);
            const lat = Number.parseFloat(latRaw);
            const lon = Number.parseFloat(lonRaw);

            const isValid = Number.isFinite(timestampMs)
                && Number.isFinite(lat)
                && Number.isFinite(lon)
                && lat >= -90
                && lat <= 90
                && lon >= -180
                && lon <= 180;

            if (!isValid) {
                continue;
            }

            points.push({ stamp, lat, lon, timestampMs });
        }

        if (points.length === 0) {
            throw new Error('No valid rows found in CSV (expected ISO stamp with tz offset, lat, lon)');
        }

        points.sort((a, b) => a.timestampMs - b.timestampMs);

        let minLat = Number.POSITIVE_INFINITY;
        let maxLat = Number.NEGATIVE_INFINITY;
        let minLon = Number.POSITIVE_INFINITY;
        let maxLon = Number.NEGATIVE_INFINITY;
        let totalDistanceKm = 0;

        for (let i = 0; i < points.length; i += 1) {
            const p = points[i];
            minLat = Math.min(minLat, p.lat);
            maxLat = Math.max(maxLat, p.lat);
            minLon = Math.min(minLon, p.lon);
            maxLon = Math.max(maxLon, p.lon);

            if (i > 0) {
                totalDistanceKm += haversineKm(points[i - 1], p);
            }
        }

        const minX = SVG_PADDING;
        const maxX = SVG_WIDTH - SVG_PADDING;
        const minY = SVG_PADDING;
        const maxY = SVG_HEIGHT - SVG_PADDING;

        const pathData = points
            .map((point, index) => {
                const x = project(point.lon, minLon, maxLon, minX, maxX);
                const y = project(point.lat, minLat, maxLat, maxY, minY);
                const prefix = index === 0 ? 'M' : 'L';
                return `${prefix}${x.toFixed(2)} ${y.toFixed(2)}`;
            })
            .join(' ');

        return {
            points,
            pathData,
            minLat,
            maxLat,
            minLon,
            maxLon,
            totalDistanceKm
        };
    }

    const trackPromise = $derived(fetchTrackData(dataUri));
</script>

{#await trackPromise}
    <div class='flex justify-center items-center'>
        <LoadingCircle />
    </div>
{:then track}
    <div class='space-y-3'>
        <div class='grid grid-cols-1 gap-2 text-xs text-gray-600 sm:grid-cols-2 lg:grid-cols-4'>
            <div><span class='font-medium text-gray-800'>Points:</span> {track.points.length}</div>
            <div><span class='font-medium text-gray-800'>Distance:</span> {track.totalDistanceKm.toFixed(3)} km</div>
            <div><span class='font-medium text-gray-800'>Start:</span> {track.points[0].stamp}</div>
            <div><span class='font-medium text-gray-800'>End:</span> {track.points[track.points.length - 1].stamp}</div>
        </div>
        <div class='rounded border border-gray-200 bg-white p-2'>
            <svg
                viewBox={`0 0 ${SVG_WIDTH} ${SVG_HEIGHT}`}
                class='h-[18rem] w-full'
                role='img'
                aria-label='GPS track preview'
            >
                <rect x='0' y='0' width={SVG_WIDTH} height={SVG_HEIGHT} fill='#f8fafc' />
                <path d={track.pathData} fill='none' stroke='#0f766e' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round' />
            </svg>
        </div>
        <div class='text-xs text-gray-500'>
            lat [{track.minLat.toFixed(6)}, {track.maxLat.toFixed(6)}], lon [{track.minLon.toFixed(6)}, {track.maxLon.toFixed(6)}]
        </div>
    </div>
{:catch error}
    <div class='rounded border border-red-200 bg-red-50 p-2 text-sm text-red-800'>{error?.message ?? 'Failed to load GPS track preview'}</div>
{/await}
