export type RunMetrics = {
  avgLatencyMs: number | null
  p95LatencyMs: number | null
  errorRate: number | null
  requestRate: number | null
  vus: number | null
  checksRate: number | null
  dataReceivedRate: number | null
  dataSentRate: number | null
}

export type RunDownloads = {
  meta: string
  summaryJson: string
  summaryTxt: string
  runLog: string
}

export type RunSummary = {
  id: string
  testName: string
  provider: string
  startedAt: string | null
  meta: Record<string, unknown>
  metrics: RunMetrics
  downloads: RunDownloads
  errors: string[]
}

export type RunComparison = {
  left: RunSummary
  right: RunSummary
  delta: RunMetrics
}

export type RunFilters = {
  testName?: string
  provider?: string
  search?: string
}

export type ResultsStore = {
  listRuns(filters?: RunFilters): Promise<RunSummary[]>
  getRun(runId: string): Promise<RunSummary | null>
  compareRuns(leftId: string, rightId: string): Promise<RunComparison | null>
  getDownloadPath(runId: string, fileName: string): Promise<string | null>
}
