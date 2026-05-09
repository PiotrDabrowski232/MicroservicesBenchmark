# Benchmark Results Viewer

Internal frontend for browsing benchmark runs produced during the microservices communication experiments. The application lists collected runs, shows a compact detail view, and compares two selected runs with numeric deltas plus a small chart area.

## Local development

```bash
npm install
npm run dev
```

The Vite dev server serves the frontend, while the local Node server exposes `/api` endpoints backed by `RESULTS_ROOT`.

## Production-style run on the VM-k6 machine

From the repository root:

```bash
chmod +x scripts/start-benchmark-results-viewer.sh
RESULTS_ROOT=${RESULTS_ROOT:-$HOME/benchmark-results} PORT=${PORT:-4173} ./scripts/start-benchmark-results-viewer.sh
```

The script installs dependencies when needed, builds the frontend, and starts the server in production-style mode.
The build step is intentional and happens on every startup, so a short startup delay is expected before the server begins listening.

After startup, the viewer is available at:

```text
http://<vm-k6-public-ip>:4173
```
