export type RunMetrics = {
  avgLatencyMs: number | null
  p95LatencyMs: number | null
  errorRate: number | null
  requestRate: number | null
  vus: number | null
  checksRate: number | null
  dataReceivedRate: number | null
  dataSentRate: number | null
  acceptanceLatencyAvgMs: number | null
  acceptanceLatencyP95Ms: number | null
  terminalLatencyAvgMs: number | null
  terminalLatencyP95Ms: number | null
  terminalResolutionRate: number | null
  businessSuccessRate: number | null
  iterations: number | null
  acceptedOrders: number | null
  completedOrders: number | null
  inventoryFailedOrders: number | null
  paymentFailedOrders: number | null
  unresolvedOrders: number | null
}

export type RunDownload = {
  label: string
  fileName: string
  href: string
}

export type RunSummary = {
  id: string
  testName: string
  provider: string
  startedAt: string | null
  meta: Record<string, unknown>
  metrics: RunMetrics
  downloads: RunDownload[]
  errors: string[]
}

export type RunComparison = {
  left: RunSummary
  right: RunSummary
  delta: RunMetrics
}
