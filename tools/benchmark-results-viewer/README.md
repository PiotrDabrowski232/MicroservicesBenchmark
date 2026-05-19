# Benchmark Results Viewer

Internal frontend for browsing benchmark runs produced during the microservices communication experiments. The application lists collected runs, shows a compact detail view, and compares two selected runs with numeric deltas plus a small chart area.

## Local development

```bash
npm install
npm run dev
```

The Vite dev server serves the frontend, while the local Node server exposes `/api` endpoints backed by `RESULTS_ROOT`.

The current repository uses a broker-centric results layout:

```text
<results-root>/
  RabbitMQResults/
  KafkaResults/
  LavinResults/
```

Each folder contains files like:

```text
async-accept-test-rabbitmq-manual-async-accept-test-summary.json
async-accept-test-rabbitmq-manual-async-accept-test-summary.txt
```

The viewer now supports both this flat layout and the older nested layout.

## Production-style run on the VM-k6 machine

From the repository root:

```bash
chmod +x scripts/start-benchmark-results-viewer.sh
RESULTS_ROOT=${RESULTS_ROOT:-$HOME/benchmark-results} PORT=${PORT:-4173} ./scripts/start-benchmark-results-viewer.sh
```

The script installs dependencies when needed, builds the frontend, and starts the server in production-style mode.
The build step is intentional and happens on every startup, so a short startup delay is expected before the server begins listening.

If `RESULTS_ROOT` is not provided, the startup flow first checks whether the repository root contains
`RabbitMQResults`, `KafkaResults` or `LavinResults`. If yes, that repository root is used automatically.
Otherwise it falls back to `$HOME/benchmark-results`.

## Metrics shown

The viewer exposes both generic HTTP metrics and async-flow metrics used in this repository:

- HTTP latency and error rate
- acceptance latency
- terminal latency
- terminal resolution rate
- business success rate
- iterations and accepted orders
- completed, inventory failed, payment failed and unresolved orders

After startup, the viewer is available at:

```text
http://<vm-k6-public-ip>:4173
```
