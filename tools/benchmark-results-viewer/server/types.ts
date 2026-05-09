export type RunMetrics = {
  avgLatencyMs: number | null
  p95LatencyMs: number | null
  errorRate: number | null
  requestRate: number | null
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
  startedAt: string
  path: string
  meta: Record<string, unknown> | null
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
