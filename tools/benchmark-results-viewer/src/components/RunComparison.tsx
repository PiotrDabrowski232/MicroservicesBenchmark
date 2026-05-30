import { useRef } from 'react'
import { toPng } from 'html-to-image'
import {
  Bar,
  BarChart,
  Cell,
  CartesianGrid,
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
  { key: 'requestRate', label: 'Request rate', unit: 'req/s', fractionDigits: 2, higherIsBetter: true },
  { key: 'vus', label: 'VUs', unit: '', fractionDigits: 0, higherIsBetter: true },
  { key: 'checksRate', label: 'Checks', unit: '%', fractionDigits: 2, isPercentage: true, higherIsBetter: true },
  { key: 'dataReceivedRate', label: 'Throughput (In)', unit: 'bytes/s', fractionDigits: 2, higherIsBetter: true },
  { key: 'dataSentRate', label: 'Throughput (Out)', unit: 'bytes/s', fractionDigits: 2, higherIsBetter: true }
]

// Keep the chart area focused on the three direct side-by-side metrics from Task 4.
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

function formatAxisValue(
  value: number,
  descriptor: Pick<MetricDescriptor, 'fractionDigits' | 'isPercentage'>
) {
  const normalizedValue = descriptor.isPercentage ? value * 100 : value
  return new Intl.NumberFormat(undefined, {
    maximumFractionDigits: descriptor.fractionDigits
  }).format(normalizedValue)
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
  const panelRef = useRef<HTMLElement>(null)
  const hasSelection = Boolean(compareLeftRun && compareRightRun)

  const handleExportPng = async () => {
    if (!panelRef.current || !comparison) return

    try {
      const dataUrl = await toPng(panelRef.current, {
        backgroundColor: '#07111f',
        style: {
          borderRadius: '0'
        }
      })
      const link = document.createElement('a')
      link.download = `comparison-${comparison.left.id}-vs-${comparison.right.id}.png`
      link.href = dataUrl
      link.click()
    } catch (err) {
      console.error('Failed to export PNG', err)
    }
  }

  return (
    <section className="comparison-panel" ref={panelRef}>
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

        <div className="comparison-actions">
          {comparison ? (
            <button type="button" className="export-button" onClick={handleExportPng}>
              Export PNG
            </button>
          ) : null}
          <button type="button" className="compare-submit" onClick={onCompare} disabled={!hasSelection || isLoading}>
            {isLoading ? 'Comparing…' : 'Compare'}
          </button>
        </div>
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
                <p className="comparison-note">Each metric uses its own chart so ms and req/s stay on separate axes.</p>
              </div>
              <span className="comparison-legend-note">Delta cards show B − A.</span>
            </div>

            <div className="comparison-chart-grid">
              {chartDescriptors.map((descriptor) => {
                const leftValue = comparison.left.metrics[descriptor.key]
                const rightValue = comparison.right.metrics[descriptor.key]
                const chartData = [
                  { run: 'A', value: leftValue, fill: '#5eead4' },
                  { run: 'B', value: rightValue, fill: '#89b4ff' }
                ]
                  .filter((entry): entry is { run: string; value: number; fill: string } => entry.value !== null)

                return (
                  <article className="comparison-mini-chart-card" key={descriptor.key}>
                    <div className="comparison-mini-chart-header">
                      <span className="metric-label">{descriptor.label}</span>
                      <span className="comparison-chart-unit">{descriptor.unit}</span>
                    </div>

                    {chartData.length > 0 ? (
                      <div className="comparison-chart comparison-chart-compact">
                        <ResponsiveContainer width="100%" height="100%">
                          <BarChart data={chartData} layout="vertical" margin={{ top: 4, right: 12, left: 4, bottom: 4 }}>
                            <CartesianGrid stroke="rgba(137, 180, 255, 0.14)" horizontal={false} />
                            <XAxis
                              type="number"
                              stroke="#aab8ce"
                              tickLine={false}
                              axisLine={false}
                              tickFormatter={(value) => formatAxisValue(value, descriptor)}
                            />
                            <YAxis
                              type="category"
                              dataKey="run"
                              stroke="#aab8ce"
                              tickLine={false}
                              axisLine={false}
                              width={28}
                            />
                            <Tooltip
                              cursor={{ fill: 'rgba(137, 180, 255, 0.08)' }}
                              formatter={(value: number) => formatValue(value, descriptor)}
                              labelFormatter={(label) => `Run ${label}`}
                              contentStyle={{
                                borderRadius: '0.85rem',
                                border: '1px solid rgba(137, 180, 255, 0.2)',
                                background: 'rgba(8, 20, 37, 0.96)',
                                color: '#e7edf7'
                              }}
                            />
                            <Bar dataKey="value" radius={[0, 8, 8, 0]}>
                              {chartData.map((entry) => (
                                <Cell key={`${descriptor.key}-${entry.run}`} fill={entry.fill} />
                              ))}
                            </Bar>
                          </BarChart>
                        </ResponsiveContainer>
                      </div>
                    ) : (
                      <div className="panel-message comparison-chart-empty-state">No values available for this metric.</div>
                    )}
                  </article>
                )
              })}
            </div>
          </section>
        </>
      ) : null}
    </section>
  )
}
