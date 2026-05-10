# Grafana Benchmark Dashboards Design

**Problem:** The benchmark environment exposes Prometheus, Grafana, Jaeger, and cAdvisor, but Grafana does not yet provide thesis-ready dashboards focused on comparing REST and gRPC runs.

**Goal:** Add a small, research-oriented Grafana dashboard set that makes benchmark runs easy to interpret, screenshot, and describe in the thesis.

## Scope

This design covers only Grafana dashboard provisioning for synchronous benchmark analysis. It does not change application logic, benchmark scripts, or Jaeger configuration.

The dashboard set must support:
- comparing REST and gRPC runs with the same scenario
- observing latency, throughput, and failure signals
- observing container CPU, memory, and network usage
- taking repeatable screenshots after each test run for the thesis

## Constraints

- Keep the solution minimal and scientific rather than operationally broad.
- Reuse the current observability stack: Prometheus, Grafana, cAdvisor, and existing service metrics.
- Avoid adding new infrastructure unless existing metrics are insufficient.
- Keep screenshots readable and comparable between runs.

## Recommended Approach

Provision three compact Grafana dashboards from JSON files stored in the repository:

1. **Request Performance**
   - focus: latency, throughput, request outcomes
   - thesis value: direct REST vs gRPC comparison

2. **Container Resource Usage**
   - focus: CPU, memory, network receive/transmit per service container
   - thesis value: resource-cost comparison for the same workload

3. **Test Run Overview**
   - focus: target health, scrape health, quick environment context during a run
   - thesis value: proves the benchmark environment was stable while measurements were collected

This approach is preferred because it keeps the dashboard set small, aligned with the thesis goal, and easy to reproduce on Azure.

## Dashboard Design

### 1. Request Performance

Purpose: compare benchmark behavior of `order-service`, `inventory-service`, and `payment-service` during a selected run window.

Panels:
- request throughput per service
- latency trend for benchmark traffic
- p95 latency panel
- failed request rate / error signal
- optional total request count for the selected range

Preferred metric sources:
- existing ASP.NET Core / OpenTelemetry / Prometheus HTTP metrics if already exposed
- fallback to any existing application request counters/histograms already available in Prometheus

Design notes:
- panels should be grouped so one screenshot captures the complete dashboard
- legends should use service names directly
- units must be explicit: milliseconds for latency, req/s for throughput, percent or rate for failures

### 2. Container Resource Usage

Purpose: show infrastructure cost of the same REST or gRPC scenario.

Panels:
- CPU usage per application container
- memory usage per application container
- network receive per application container
- network transmit per application container

Metric source:
- cAdvisor metrics already scraped by Prometheus

Scope of containers:
- `order-service`
- `inventory-service`
- `payment-service`

Design notes:
- infrastructure containers such as Grafana, Prometheus, RabbitMQ, Kafka, and phpMyAdmin should not dominate the default view
- panels should make side-by-side comparison between the three business services easy

### 3. Test Run Overview

Purpose: provide a compact environment-stability snapshot for each run.

Panels:
- Prometheus scrape health / target availability
- container up-state for the three benchmark services
- optional overall container count in the benchmark stack
- optional quick panel for recent request rate, if available

Design notes:
- this dashboard is secondary; it supports methodology screenshots and troubleshooting
- it should remain smaller than the first two dashboards

## Screenshot Workflow

For each benchmark scenario:
1. run the test from `vm-benchmark-k6`
2. open Grafana with a short time range covering only that run
3. capture:
   - full **Request Performance** dashboard
   - full **Container Resource Usage** dashboard
4. open Jaeger and capture one representative trace for the same scenario

Suggested screenshot naming:
- `rest-transport-ping-grafana-request-performance.png`
- `rest-transport-ping-grafana-resource-usage.png`
- `rest-transport-ping-jaeger-trace.png`

The same convention should be used for `grpc`, `payload-100`, `payload-1000`, and `stress`.

## Non-Goals

- building a full production monitoring suite
- adding alerting
- introducing unrelated exporters
- redesigning Jaeger UI or trace instrumentation

## Risks and Mitigations

- **Risk:** application latency metrics may be missing or too weak for a useful request dashboard  
  **Mitigation:** first inspect available Prometheus series and build panels only from real metrics; add instrumentation only if strictly necessary.

- **Risk:** dashboards become cluttered and weak for thesis screenshots  
  **Mitigation:** keep the set to three dashboards and prefer fewer, clearer panels.

- **Risk:** screenshots from different runs are not comparable  
  **Mitigation:** keep fixed dashboard layouts, fixed panel order, and a consistent naming convention.

## Success Criteria

- Grafana automatically loads the benchmark dashboards from repository-managed files.
- The dashboards show usable data for REST and gRPC test runs.
- The user can take consistent screenshots after each benchmark run.
- The dashboard set directly supports the thesis discussion of latency, throughput, failures, and resource usage.
