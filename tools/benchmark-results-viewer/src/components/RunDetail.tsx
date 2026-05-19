import type { RunSummary } from '../types'
import { formatDateTime } from '../utils/date'

type RunDetailProps = {
  run: RunSummary | null
  isLoading: boolean
  error: string | null
  isCompareLeft: boolean
  isCompareRight: boolean
}

function formatMetric(value: number | null, maximumFractionDigits = 2, suffix = '') {
  if (value === null) {
    return 'N/A'
  }

  return `${new Intl.NumberFormat(undefined, {
    maximumFractionDigits
  }).format(value)}${suffix}`
}

function formatPercentage(value: number | null) {
  if (value === null) {
    return 'N/A'
  }

  return `${new Intl.NumberFormat(undefined, {
    style: 'percent',
    maximumFractionDigits: 2
  }).format(value)}`
}

export function RunDetail({
  run,
  isLoading,
  error,
  isCompareLeft,
  isCompareRight
}: RunDetailProps) {
  if (error) {
    return <section className="detail-panel panel-message panel-message-error">{error}</section>
  }

  if (isLoading) {
    return <section className="detail-panel panel-message">Loading run details…</section>
  }

  if (!run) {
    return (
      <section className="detail-panel detail-empty-state">
        <h2>Select a run</h2>
        <p>Pick a benchmark run from the list to inspect metrics, metadata and download files.</p>
      </section>
    )
  }

  return (
    <section className="detail-panel">
      <header className="detail-header">
        <div>
          <p className="eyebrow">Run detail</p>
          <h2>{run.id}</h2>
          <p className="detail-subtitle">
            {run.provider.toUpperCase()} · {run.testName}
          </p>
          <p className="detail-started-at">
            {formatDateTime(run.startedAt, {
              dateStyle: 'full',
              timeStyle: 'medium'
            })}
          </p>
        </div>

        <div className="detail-badges">
          {isCompareLeft ? <span className="detail-badge">Compare A</span> : null}
          {isCompareRight ? <span className="detail-badge detail-badge-secondary">Compare B</span> : null}
        </div>
      </header>

      <div className="metric-grid">
        <article className="metric-card">
          <span className="metric-label">HTTP avg latency</span>
          <strong>{formatMetric(run.metrics.avgLatencyMs, 2, ' ms')}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">HTTP p95 latency</span>
          <strong>{formatMetric(run.metrics.p95LatencyMs, 2, ' ms')}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Acceptance avg</span>
          <strong>{formatMetric(run.metrics.acceptanceLatencyAvgMs, 2, ' ms')}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Acceptance p95</span>
          <strong>{formatMetric(run.metrics.acceptanceLatencyP95Ms, 2, ' ms')}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Terminal avg</span>
          <strong>{formatMetric(run.metrics.terminalLatencyAvgMs, 2, ' ms')}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Terminal p95</span>
          <strong>{formatMetric(run.metrics.terminalLatencyP95Ms, 2, ' ms')}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Resolution rate</span>
          <strong>{formatPercentage(run.metrics.terminalResolutionRate)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Business success</span>
          <strong>{formatPercentage(run.metrics.businessSuccessRate)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">HTTP error rate</span>
          <strong>{formatPercentage(run.metrics.errorRate)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Request rate</span>
          <strong>{formatMetric(run.metrics.requestRate, 2, ' req/s')}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">VUs</span>
          <strong>{formatMetric(run.metrics.vus, 0)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Checks</span>
          <strong>{formatPercentage(run.metrics.checksRate)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Iterations</span>
          <strong>{formatMetric(run.metrics.iterations, 0)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Accepted orders</span>
          <strong>{formatMetric(run.metrics.acceptedOrders, 0)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Completed orders</span>
          <strong>{formatMetric(run.metrics.completedOrders, 0)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Inventory failed</span>
          <strong>{formatMetric(run.metrics.inventoryFailedOrders, 0)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Payment failed</span>
          <strong>{formatMetric(run.metrics.paymentFailedOrders, 0)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Unresolved orders</span>
          <strong>{formatMetric(run.metrics.unresolvedOrders, 0)}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Throughput (In)</span>
          <strong>{formatMetric(run.metrics.dataReceivedRate, 2, ' bytes/s')}</strong>
        </article>
        <article className="metric-card">
          <span className="metric-label">Throughput (Out)</span>
          <strong>{formatMetric(run.metrics.dataSentRate, 2, ' bytes/s')}</strong>
        </article>
      </div>

      <div className="detail-sections">
        <section className="detail-section">
          <div className="section-heading">
            <h3>Metadata</h3>
          </div>
          <pre className="json-block">{JSON.stringify(run.meta, null, 2)}</pre>
        </section>

        <section className="detail-section">
          <div className="section-heading">
            <h3>Downloads</h3>
          </div>
          <div className="download-list">
            {run.downloads.map((download) => (
              <a href={download.href} key={download.fileName}>
                {download.label}
              </a>
            ))}
          </div>
        </section>

        {run.errors.length > 0 ? (
          <section className="detail-section">
            <div className="section-heading">
              <h3>Parsing issues</h3>
            </div>
            <ul className="error-list">
              {run.errors.map((issue) => (
                <li key={issue}>{issue}</li>
              ))}
            </ul>
          </section>
        ) : null}
      </div>
    </section>
  )
}
