import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis
} from 'recharts'
import type { RunComparison as RunComparisonType, RunSummary } from '../types'

type RunComparisonProps = {
  comparison: RunComparisonType | null
  compareLeftRun: RunSummary | null
  compareRightRun: RunSummary | null
  isLoading: boolean
  error: string | null
  onCompare: () => void
}

type MetricDescriptor = {
  key: keyof RunComparisonType['delta']
  label: string
  unit: string
  fractionDigits: number
  isPercentage?: boolean
  higherIsBetter: boolean
}

const metricDescriptors: MetricDescriptor[] = [
  { key: 'avgLatencyMs', label: 'Avg latency', unit: 'ms', fractionDigits: 2, higherIsBetter: false },
  { key: 'p95LatencyMs', label: 'P95 latency', unit: 'ms', fractionDigits: 2, higherIsBetter: false },
  { key: 'errorRate', label: 'Error rate', unit: '%', fractionDigits: 2, isPercentage: true, higherIsBetter: false },
  { key: 'requestRate', label: 'Request rate', unit: 'req/s', fractionDigits: 2, higherIsBetter: true }
]

// Exclude error rate here because it is already emphasized in the delta cards and would skew the shared scale.
const chartDescriptors = metricDescriptors.filter(({ key }) => key !== 'errorRate')

function formatValue(
  value: number | null,
  descriptor: Pick<MetricDescriptor, 'fractionDigits' | 'unit' | 'isPercentage'>
) {
  if (value === null) {
    return 'N/A'
  }

  if (descriptor.isPercentage) {
    return `${new Intl.NumberFormat(undefined, {
      maximumFractionDigits: descriptor.fractionDigits,
      minimumFractionDigits: 0
    }).format(value * 100)} ${descriptor.unit}`
  }

  return `${new Intl.NumberFormat(undefined, {
    maximumFractionDigits: descriptor.fractionDigits
  }).format(value)} ${descriptor.unit}`
}

function formatDelta(
  value: number | null,
  descriptor: Pick<MetricDescriptor, 'fractionDigits' | 'unit' | 'isPercentage'>
) {
  if (value === null) {
    return 'N/A'
  }

  const prefix = value > 0 ? '+' : ''
  if (descriptor.isPercentage) {
    return `${prefix}${new Intl.NumberFormat(undefined, {
      maximumFractionDigits: descriptor.fractionDigits,
      minimumFractionDigits: 0
    }).format(value * 100)} ${descriptor.unit}`
  }

  return `${prefix}${new Intl.NumberFormat(undefined, {
    maximumFractionDigits: descriptor.fractionDigits
  }).format(value)} ${descriptor.unit}`
}

function getDeltaClassName(deltaValue: number | null, descriptor: MetricDescriptor) {
  if (deltaValue === null || deltaValue === 0) {
    return 'comparison-delta'
  }

  const isBetter = descriptor.higherIsBetter ? deltaValue > 0 : deltaValue < 0
  return `comparison-delta ${isBetter ? 'is-better' : 'is-worse'}`
}

function describeRun(run: RunSummary | null) {
  if (!run) {
    return 'Not selected'
  }

  return `${run.id} · ${run.provider.toUpperCase()} · ${run.testName}`
}

export function RunComparison({
  comparison,
  compareLeftRun,
  compareRightRun,
  isLoading,
  error,
  onCompare
}: RunComparisonProps) {
  const hasSelection = Boolean(compareLeftRun && compareRightRun)
  const chartData = comparison
    ? chartDescriptors.map((descriptor) => ({
        metric: descriptor.label,
        left: comparison.left.metrics[descriptor.key],
        right: comparison.right.metrics[descriptor.key]
      }))
    : []

  return (
    <section className="comparison-panel">
      <header className="comparison-header">
        <div>
          <p className="eyebrow">Run comparison</p>
          <h2>
            {compareLeftRun?.id ?? 'Run A'} vs {compareRightRun?.id ?? 'Run B'}
          </h2>
          <p className="comparison-subtitle">
            <span>A: {describeRun(compareLeftRun)}</span>
            <span>B: {describeRun(compareRightRun)}</span>
          </p>
        </div>

        <button type="button" className="compare-submit" onClick={onCompare} disabled={!hasSelection || isLoading}>
          {isLoading ? 'Comparing…' : 'Compare'}
        </button>
      </header>

      {error ? <div className="panel-message panel-message-error">{error}</div> : null}

      {!hasSelection ? (
        <div className="panel-message comparison-empty-state">
          Select both compare A and compare B to load a side-by-side summary.
        </div>
      ) : null}

      {hasSelection && !error && !isLoading && !comparison ? (
        <div className="panel-message comparison-empty-state">
          Comparison is ready to load. Click Compare to fetch the current backend summary.
        </div>
      ) : null}

      {hasSelection && comparison ? (
        <>
          <div className="comparison-delta-grid">
            {metricDescriptors.map((descriptor) => {
              const deltaValue = comparison.delta[descriptor.key]

              return (
                <article className="comparison-card" key={descriptor.key}>
                  <span className="metric-label">{descriptor.label}</span>
                  <strong className={getDeltaClassName(deltaValue, descriptor)}>
                    {formatDelta(deltaValue, descriptor)}
                  </strong>
                  <div className="comparison-values">
                    <span>A: {formatValue(comparison.left.metrics[descriptor.key], descriptor)}</span>
                    <span>B: {formatValue(comparison.right.metrics[descriptor.key], descriptor)}</span>
                  </div>
                </article>
              )
            })}
          </div>

          <section className="comparison-chart-card">
            <div className="section-heading comparison-chart-heading">
              <div>
                <h3>Metric comparison</h3>
                <p className="comparison-note">Bars compare direct values for runs A and B.</p>
              </div>
              <span className="comparison-legend-note">Delta cards show B − A.</span>
            </div>

            <div className="comparison-chart">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chartData} barGap={10}>
                  <CartesianGrid stroke="rgba(137, 180, 255, 0.14)" vertical={false} />
                  <XAxis dataKey="metric" stroke="#aab8ce" tickLine={false} axisLine={false} />
                  <YAxis stroke="#aab8ce" tickLine={false} axisLine={false} width={72} />
                  <Tooltip
                    cursor={{ fill: 'rgba(137, 180, 255, 0.08)' }}
                    contentStyle={{
                      borderRadius: '0.85rem',
                      border: '1px solid rgba(137, 180, 255, 0.2)',
                      background: 'rgba(8, 20, 37, 0.96)',
                      color: '#e7edf7'
                    }}
                  />
                  <Legend />
                  <Bar dataKey="left" name="Run A" fill="#5eead4" radius={[8, 8, 0, 0]} />
                  <Bar dataKey="right" name="Run B" fill="#89b4ff" radius={[8, 8, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </section>
        </>
      ) : null}
    </section>
  )
}
