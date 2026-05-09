# Benchmark Results Viewer Design

**Problem:** Benchmark runs saved on the k6 VM already produce useful artifacts (`meta.json`, `summary.json`, `summary.txt`, `run.log`), but reviewing them currently requires manual shell navigation and ad hoc file inspection.

**Goal:** Add a lightweight web application on the k6 VM that reads the existing `~/benchmark-results` directory, lets the user browse saved runs, compare two runs, view simple charts for key metrics, and download raw result files.

## Scope

This design covers a read-only benchmark results viewer deployed on the k6 VM.

The first version must support:
- listing saved runs from `~/benchmark-results`
- filtering runs by test name and provider
- viewing one run in detail
- comparing two runs side by side
- rendering simple visualizations for key metrics
- downloading `meta.json`, `summary.json`, `summary.txt`, and `run.log`

This design does not include:
- starting or stopping benchmark runs from the UI
- editing or deleting run files
- storing results in a database
- authentication and authorization

## Constraints

- Reuse the current file-based benchmark pipeline without changing the output format.
- Keep the application lightweight and easy to run directly on the k6 VM.
- Prefer simple operational behavior over production-grade platform features.
- Public access is acceptable for the first version, but the application should remain read-only.
- The UI must be useful for thesis work, especially quick review and screenshot-friendly comparison.

## Recommended Approach

Build a small two-part application:

1. **React frontend**
   - presents the list, detail view, comparison view, and charts
   - consumes only HTTP API responses

2. **Node.js backend**
   - scans `~/benchmark-results`
   - reads and normalizes benchmark files
   - exposes run metadata, summary metrics, comparison payloads, and download links
   - serves the built frontend and downloadable files

This approach is preferred because it keeps file-system access on the server, minimizes changes to the benchmark pipeline, and provides a clean path for future extensions such as better filtering or additional visualizations.

## User Experience

### Main Layout

The interface uses a two-column layout:
- left sidebar: filters and run list
- right content area: selected run details and comparison tools

The detail view and comparison view share the same page so the user can quickly move from run selection to analysis without navigation overhead.

### Run List

The sidebar shows available runs grouped by test directory, ordered by newest first.

Supported filters:
- test name (`transport-ping`, `payload`, `stress`, later additional tests)
- provider (`rest`, `grpc`, optional fallback for other run types)
- text search by run name

Each list item should display at least:
- run name
- test name
- provider
- start time

### Run Detail View

Selecting a run loads:
- summary cards for `avg latency`, `p95 latency`, `error rate`, and `request rate`
- run metadata from `meta.json`
- a compact table of selected metrics from `summary.json`
- download actions for all raw files

The detail view should also expose the chosen run as candidate A or B for comparison.

### Comparison View

The user can select two runs and view them side by side.

The comparison view should show:
- shared context (test name, provider, timestamp)
- metric delta cards for `avg latency`, `p95 latency`, `error rate`, and `request rate`
- a compact comparison table
- simple charts that make REST vs gRPC differences easy to scan

The first version should not try to compare arbitrary deep metric trees. It should focus on a curated set of thesis-relevant metrics.

## Data Model

The backend should normalize each run into a compact model similar to:

```json
{
  "id": "transport-ping-rest-2026-05-06T16-52-51",
  "testName": "transport-ping",
  "provider": "rest",
  "startedAt": "2026-05-06T16:52:51Z",
  "path": "/home/azureuser/benchmark-results/transport-ping/transport-ping-rest-2026-05-06T16-52-51",
  "meta": {
    "azureRegion": "PolandCentral",
    "systemVmName": "vm-benchmark-system",
    "k6VmName": "vm-benchmark-k6",
    "baseUrl": "http://10.10.1.4:5001"
  },
  "metrics": {
    "avgLatencyMs": 2.28,
    "p95LatencyMs": 3.31,
    "errorRate": 0,
    "requestRate": 16.19
  },
  "downloads": {
    "meta": "/api/runs/transport-ping-rest-2026-05-06T16-52-51/files/meta.json",
    "summaryJson": "/api/runs/transport-ping-rest-2026-05-06T16-52-51/files/summary.json",
    "summaryTxt": "/api/runs/transport-ping-rest-2026-05-06T16-52-51/files/summary.txt",
    "runLog": "/api/runs/transport-ping-rest-2026-05-06T16-52-51/files/run.log"
  }
}
```

If a metric is missing in `summary.json`, the backend should return `null` for that field instead of failing the whole run.

## API Design

The first version should expose these endpoints:

- `GET /api/runs`
  - returns the normalized run list
  - supports optional query params: `testName`, `provider`, `search`

- `GET /api/runs/:id`
  - returns full details for one run
  - includes normalized key metrics and raw metadata

- `GET /api/runs/compare?left=:id&right=:id`
  - returns two normalized runs plus a precomputed comparison block

- `GET /api/runs/:id/files/:name`
  - downloads one of the allowed files:
    - `meta.json`
    - `summary.json`
    - `summary.txt`
    - `run.log`

The backend should never expose arbitrary filesystem reads outside the benchmark results directory.

## Visualization Design

The first version should include a small number of simple charts:

1. latency comparison chart
   - bars for `avg` and `p95`

2. reliability chart
   - bar or badge for `error rate`

3. throughput chart
   - bar for request rate

Charts should stay compact and secondary to the numeric values. This is an analysis helper, not a replacement for Grafana.

## Error Handling

- If a run directory is incomplete, the run should still appear in the list if `meta.json` exists or the directory name is parseable.
- Missing files should be surfaced as partial data, not hidden silently.
- Invalid JSON in a result file should produce a visible error state for that run only.
- File download requests for unsupported file names should return `404`.
- If the results directory does not exist, the API should return an empty list and a clear status message for the frontend.

The system should fail explicitly per run rather than masking parsing problems across all runs.

## Deployment Model

The application will run directly on the k6 VM as a separate lightweight web process.

Deployment assumptions:
- Node.js is installed on the k6 VM
- the app runs under a dedicated folder in the repository or a small sibling folder
- the backend reads the benchmark results root from configuration, defaulting to `~/benchmark-results`
- the server listens on a fixed port that can be opened publicly for the user

This design intentionally avoids adding the viewer to the main Docker Compose benchmark stack because the viewer is an operator tool tied to the k6 VM and its local result files.

## Testing Strategy

The implementation plan should include:
- backend unit tests for run discovery and metric normalization
- backend tests for safe file download behavior
- a minimal frontend smoke test for list/detail/compare rendering
- a manual verification flow on the k6 VM with real saved runs

The first version does not require end-to-end browser automation if the backend parsing and manual rendering flow are verified.

## Alternatives Considered

### 1. Static frontend plus generated index file

Pros:
- simplest hosting model
- very low runtime complexity

Cons:
- requires regenerating an index after each new benchmark run
- weaker fit for immediate visibility of newly added results

### 2. Plain server-rendered HTML without React

Pros:
- smaller implementation
- minimal dependencies

Cons:
- less flexible comparison UI
- harder to evolve visualizations cleanly

### 3. Full dashboard with benchmark execution controls

Pros:
- one place to run and review tests

Cons:
- broader scope
- mixes experiment execution with result analysis
- significantly higher operational and security risk

## Risks and Mitigations

- **Risk:** result file shapes vary slightly between scripts or future benchmark types  
  **Mitigation:** normalize only the required metric subset and keep unsupported fields optional.

- **Risk:** public unauthenticated access exposes run logs  
  **Mitigation:** keep the viewer read-only, restrict downloads to the known files, and consider network-level restriction later if needed.

- **Risk:** parsing all runs on every request becomes slow as the result set grows  
  **Mitigation:** start with simple on-demand reads; if needed later, add lightweight in-memory caching without changing the API.

## Success Criteria

- The application runs on the k6 VM and reads the existing `~/benchmark-results` structure.
- The user can browse runs, open one run, compare two runs, and download raw files.
- The UI highlights `avg latency`, `p95 latency`, `error rate`, and `request rate`.
- The solution works without changing the current benchmark result pipeline.
