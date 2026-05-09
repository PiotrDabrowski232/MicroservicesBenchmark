import { promises as fs } from 'fs'
import path from 'path'
import type { RunComparison, RunFilters, RunMetrics, RunSummary, ResultsStore } from './types'

const allowedDownloadFiles = new Set(['meta.json', 'summary.json', 'summary.txt', 'run.log'])

type ParsedRun = {
  id: string
  testName: string
  provider: string
  startedAt: string | null
  filePath: string
  meta: Record<string, unknown>
  summary: Record<string, unknown> | null
  metrics: RunMetrics
  errors: string[]
  mtimeMs: number
}

type RunDirectory = {
  testDir: string
  runDir: string
}

function toText(value: unknown): string {
  return typeof value === 'string' ? value : ''
}

function asRecord(value: unknown): Record<string, unknown> | null {
  if (value && typeof value === 'object' && !Array.isArray(value)) {
    return value as Record<string, unknown>
  }

  return null
}

// syncProvider is expected in meta.json for new runs.
// For older runs we infer the provider from the run ID convention:
// "-rest-" maps to REST, "-grpc-" maps to gRPC, otherwise "unknown".
function normalizeProvider(value: unknown, runId: string): string {
  const fromMeta = toText(value).trim().toLowerCase()
  if (fromMeta) {
    return fromMeta
  }

  if (runId.includes('-rest-')) {
    return 'rest'
  }

  if (runId.includes('-grpc-')) {
    return 'grpc'
  }

  return 'unknown'
}

function parseStartedAt(value: unknown, runId: string): string | null {
  const raw = toText(value).trim()
  if (raw) {
    const parsed = Date.parse(raw)
    if (!Number.isNaN(parsed)) {
      return new Date(parsed).toISOString()
    }
  }

  const match = runId.match(/(\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2})$/)
  if (match) {
    const isoLike = `${match[1].replace(/T(\d{2})-(\d{2})-(\d{2})$/, 'T$1:$2:$3')}Z`
    const parsed = Date.parse(isoLike)
    if (!Number.isNaN(parsed)) {
      return new Date(parsed).toISOString()
    }
  }

  return null
}

function toNumberOrNull(value: unknown): number | null {
  if (typeof value === 'number' && Number.isFinite(value)) {
    return value
  }

  if (typeof value === 'string' && value.trim() !== '') {
    const parsed = Number(value)
    return Number.isFinite(parsed) ? parsed : null
  }

  return null
}

function isInsideRoot(rootPath: string, candidatePath: string): boolean {
  const relativePath = path.relative(rootPath, candidatePath)
  return relativePath !== '' && !relativePath.startsWith('..') && !path.isAbsolute(relativePath)
}

function extractMetrics(summary: Record<string, unknown> | null): RunMetrics {
  const metrics = asRecord(summary?.metrics)
  const httpReqDuration = asRecord(metrics?.http_req_duration)
  const httpReqFailed = asRecord(metrics?.http_req_failed)
  const httpReqs = asRecord(metrics?.http_reqs)

  return {
    avgLatencyMs: toNumberOrNull(httpReqDuration?.avg),
    p95LatencyMs: toNumberOrNull(httpReqDuration?.['p(95)']),
    errorRate: toNumberOrNull(httpReqFailed?.rate),
    requestRate: toNumberOrNull(httpReqs?.rate)
  }
}

function createDownloads(runId: string): RunSummary['downloads'] {
  return {
    meta: `/api/runs/${encodeURIComponent(runId)}/files/meta.json`,
    summaryJson: `/api/runs/${encodeURIComponent(runId)}/files/summary.json`,
    summaryTxt: `/api/runs/${encodeURIComponent(runId)}/files/summary.txt`,
    runLog: `/api/runs/${encodeURIComponent(runId)}/files/run.log`
  }
}

async function safeReadJson(filePath: string): Promise<{ value: Record<string, unknown> | null; error: string | null }> {
  const fileName = path.basename(filePath)
  try {
    const raw = await fs.readFile(filePath, 'utf8')
    const parsed = JSON.parse(raw)
    const value = asRecord(parsed)
    if (!value) {
      return { value: null, error: `${fileName} could not be parsed.` }
    }

    return { value, error: null }
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === 'ENOENT') {
      return { value: null, error: null }
    }

    if (error instanceof SyntaxError) {
      return { value: null, error: `${fileName} could not be parsed.` }
    }

    return { value: null, error: `${fileName} could not be read.` }
  }
}

async function readRunDirectory(runDir: RunDirectory): Promise<ParsedRun | null> {
  const stat = await fs.stat(runDir.runDir).catch(() => null)
  if (!stat || !stat.isDirectory()) {
    return null
  }

  const runId = path.basename(runDir.runDir)
  const testName = path.basename(runDir.testDir)
  const metaResult = await safeReadJson(path.join(runDir.runDir, 'meta.json'))
  const summaryResult = await safeReadJson(path.join(runDir.runDir, 'summary.json'))
  const meta = metaResult.value ?? {}
  const summary = summaryResult.value
  const provider = normalizeProvider(meta?.syncProvider, runId)
  const startedAt = parseStartedAt(meta?.startedAt, runId)

  return {
    id: runId,
    testName: toText(meta?.testName) || testName,
    provider,
    startedAt,
    filePath: runDir.runDir,
    meta,
    summary,
    metrics: extractMetrics(summary),
    errors: [metaResult.error, summaryResult.error].filter((entry): entry is string => Boolean(entry)),
    mtimeMs: stat.mtimeMs
  }
}

async function discoverRunDirectories(resultsRoot: string): Promise<RunDirectory[]> {
  const testDirs = await fs.readdir(resultsRoot, { withFileTypes: true }).catch(() => [])
  const discovered: RunDirectory[] = []

  for (const entry of testDirs) {
    if (!entry.isDirectory()) {
      continue
    }

    const testDir = path.join(resultsRoot, entry.name)
    const runDirs = await fs.readdir(testDir, { withFileTypes: true }).catch(() => [])
    for (const runEntry of runDirs) {
      if (!runEntry.isDirectory()) {
        continue
      }

      discovered.push({
        testDir,
        runDir: path.join(testDir, runEntry.name)
      })
    }
  }

  return discovered
}

function matchesFilters(run: RunSummary, filters?: RunFilters): boolean {
  if (!filters) {
    return true
  }

  const normalizedTestName = filters.testName?.trim().toLowerCase()
  if (normalizedTestName && run.testName.toLowerCase() !== normalizedTestName) {
    return false
  }

  const normalizedProvider = filters.provider?.trim().toLowerCase()
  if (normalizedProvider && run.provider.toLowerCase() !== normalizedProvider) {
    return false
  }

  const search = filters.search?.trim().toLowerCase()
  if (!search) {
    return true
  }

  const haystack = [
    run.id,
    run.testName,
    run.provider,
    run.startedAt,
    JSON.stringify(run.meta ?? {})
  ]
    .join(' ')
    .toLowerCase()

  return haystack.includes(search)
}

function toSummary(run: ParsedRun): RunSummary {
  return {
    id: run.id,
    testName: run.testName,
    provider: run.provider,
    startedAt: run.startedAt,
    meta: run.meta,
    metrics: run.metrics,
    downloads: createDownloads(run.id),
    errors: run.errors
  }
}

function compareMetrics(left: RunMetrics, right: RunMetrics): RunMetrics {
  const subtract = (lhs: number | null, rhs: number | null): number | null => {
    if (lhs === null || rhs === null) {
      return null
    }

    return rhs - lhs
  }

  return {
    avgLatencyMs: subtract(left.avgLatencyMs, right.avgLatencyMs),
    p95LatencyMs: subtract(left.p95LatencyMs, right.p95LatencyMs),
    errorRate: subtract(left.errorRate, right.errorRate),
    requestRate: subtract(left.requestRate, right.requestRate)
  }
}

export function createResultsStore(resultsRoot: string): ResultsStore {
  async function loadRuns(): Promise<ParsedRun[]> {
    const directories = await discoverRunDirectories(resultsRoot)
    const parsedRuns = await Promise.all(directories.map(readRunDirectory))

    return parsedRuns
      .filter((run): run is ParsedRun => Boolean(run))
      .sort((left, right) => {
        const leftTime = left.startedAt ? Date.parse(left.startedAt) : left.mtimeMs
        const rightTime = right.startedAt ? Date.parse(right.startedAt) : right.mtimeMs
        const timeDiff = rightTime - leftTime
        if (timeDiff !== 0) {
          return timeDiff
        }

        return right.mtimeMs - left.mtimeMs
      })
  }

  async function findRun(runId: string): Promise<ParsedRun | null> {
    const runs = await loadRuns()
    return runs.find((run) => run.id === runId) ?? null
  }

  return {
    async listRuns(filters?: RunFilters) {
      const runs = await loadRuns()
      return runs.map(toSummary).filter((run) => matchesFilters(run, filters))
    },

    getRun(runId) {
      return findRun(runId).then((run) => (run ? toSummary(run) : null))
    },

    async compareRuns(leftId, rightId) {
      const runs = await loadRuns()
      const left = runs.find((run) => run.id === leftId) ?? null
      const right = runs.find((run) => run.id === rightId) ?? null
      if (!left || !right) {
        return null
      }

      return {
        left: toSummary(left),
        right: toSummary(right),
        delta: compareMetrics(left.metrics, right.metrics)
      }
    },

    async getDownloadPath(runId, fileName) {
      const resolvedResultsRoot = await fs.realpath(resultsRoot).catch(() => path.resolve(resultsRoot))

      if (!allowedDownloadFiles.has(fileName)) {
        return null
      }

      const run = await findRun(runId)
      if (!run) {
        return null
      }

      const downloadPath = path.join(run.filePath, fileName)
      const resolvedDownloadPath = await fs.realpath(downloadPath).catch(() => null)
      if (!resolvedDownloadPath || !isInsideRoot(resolvedResultsRoot, resolvedDownloadPath)) {
        return null
      }

      return resolvedDownloadPath
    }
  }
}
