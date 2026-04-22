# RDPMS Web UI

Svelte 5 + Vite 6 frontend for RDPMS.

## Docs Notes

- The developer documentation includes a visualization plugin registry:
  - `docs/src/dev_section/visualization-plugin-registry.md`

## Development

Preferred workflow: run the dev server inside the `rdpms-web-ui` container from `rdpms-backend/compose.yaml`.

From the repository root:

```bash
docker compose -f rdpms-backend/compose.yaml --profile deps up rdpms-web-ui
```

The app is then available at `http://localhost:5173`.

The source tree is bind-mounted into the container, so edit files locally as usual. For checks and builds, prefer running `npm` inside the container:

```bash
docker compose -f rdpms-backend/compose.yaml --profile deps exec rdpms-web-ui npm run check
docker compose -f rdpms-backend/compose.yaml --profile deps exec rdpms-web-ui npm run lint
docker compose -f rdpms-backend/compose.yaml --profile deps exec rdpms-web-ui npm run build
```

## Host-side Fallback

If you intentionally want to run the frontend outside the container:

```bash
cd rdpms-web-ui
npm install
npm run dev
```

## Building

For a production build inside the container:

```bash
docker compose -f rdpms-backend/compose.yaml --profile deps exec rdpms-web-ui npm run build
```
