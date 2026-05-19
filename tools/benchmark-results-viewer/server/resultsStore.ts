import { promises as fs } from 'fs'
import path from 'path'
import type { RunComparison, RunDownload, RunFilters, RunMetrics, RunSummary, ResultsStore } from './types'

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
  downloads: RunDownload[]
  downloadFileNames: string[]
  mtimeMs: number
}

type LegacyRunDirectory = {
  kind: 'legacy'
  testDir: string
  runDir: string
}

type FlatRunGroup = {
  kind: 'flat'
  parentDir: string
  baseName: string
  files: string[]
  providerHint: string
}

type RunSource = LegacyRunDirectory | FlatRunGroup

function toText(value: unknown): string {
  return typeof value === 'string' ? value : ''
}

function asRecord(value: unknown): Record<string, unknown> | null {
  if (value && typeof value === 'object' && !Array.isArray(value)) {
    return value as Record<string, unknown>
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

function parseSummaryTextMeta(raw: string): Record<string, unknown> {
  const meta: Record<string, unknown> = {}

  for (const line of raw.split(/\r?\n/)) {
    const separatorIndex = line.indexOf(':')
    if (separatorIndex <= 0) {
      continue
    }

    const key = line.slice(0, separatorIndex).trim()
    const value = line.slice(separatorIndex + 1).trim()
    if (!key) {
      continue
    }

    meta[key] = value
  }

  return meta
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

async function safeReadText(filePath: string): Promise<{ value: string | null; error: string | null }> {
  const fileName = path.basename(filePath)

  try {
    const raw = await fs.readFile(filePath, 'utf8')
    return { value: raw, error: null }
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === 'ENOENT') {
      return { value: null, error: null }
    }

    return { value: null, error: `${fileName} could not be read.` }
  }
}

function normalizeProvider(value: unknown, runId: string, providerHint?: string): string {
  const fromMeta = toText(value).trim().toLowerCase()
  if (fromMeta) {
    return fromMeta
  }

  const normalizedHint = (providerHint ?? '').trim().toLowerCase()
  if (normalizedHint) {
    return normalizedHint.replace(/results$/, '')
  }

  if (runId.includes('rabbitmq')) {
    return 'rabbitmq'
  }

  if (runId.includes('lavin')) {
    return 'lavinmq'
  }

  if (runId.includes('kafka')) {
    return 'kafka'
  }

  if (runId.includes('-rest-')) {
    return 'rest'
  }

  if (runId.includes('-grpc-')) {
    return 'grpc'
  }

  return 'unknown'
}

function parseStartedAt(value: unknown, runId: string, mtimeMs: number): string | null {
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

  return Number.isFinite(mtimeMs) ? new Date(mtimeMs).toISOString() : null
}

function emptyMetrics(): RunMetrics {
  return {
    avgLatencyMs: null,
    p95LatencyMs: null,
    errorRate: null,
    requestRate: null,
    vus: null,
    checksRate: null,
    dataReceivedRate: null,
    dataSentRate: null,
    acceptanceLatencyAvgMs: null,
    acceptanceLatencyP95Ms: null,
    terminalLatencyAvgMs: null,
    terminalLatencyP95Ms: null,
    terminalResolutionRate: null,
    businessSuccessRate: null,
    iterations: null,
    acceptedOrders: null,
    completedOrders: null,
    inventoryFailedOrders: null,
    paymentFailedOrders: null,
    unresolvedOrders: null
  }
}

function extractMetrics(summary: Record<string, unknown> | null): RunMetrics {
  if (!summary) {
    return emptyMetrics()
  }

  const metrics = asRecord(summary.metrics)
  if (!metrics) {
    return emptyMetrics()
  }

  const valuesOf = (metricName: string) => asRecord(asRecord(metrics[metricName])?.values)

  const httpReqDuration = valuesOf('http_req_duration')
  const httpReqFailed = valuesOf('http_req_failed')
  const httpReqs = valuesOf('http_reqs')
  const vusMax = valuesOf('vus_max')
  const checks = valuesOf('checks')
  const dataReceived = valuesOf('data_received')
  const dataSent = valuesOf('data_sent')
  const orderAcceptanceLatency = valuesOf('order_acceptance_latency')
  const orderTerminalLatency = valuesOf('order_terminal_latency')
  const orderTerminalResolutionRate = valuesOf('order_terminal_resolution_rate')
  const orderBusinessSuccessRate = valuesOf('order_business_success_rate')
  const iterations = valuesOf('iterations')
  const acceptedOrders = valuesOf('orders_accepted_total')
  const completedOrders = valuesOf('orders_completed_total')
  const inventoryFailedOrders = valuesOf('orders_inventory_failed_total')
  const paymentFailedOrders = valuesOf('orders_payment_failed_total')
  const unresolvedOrders = valuesOf('orders_unresolved_total')

  return {
    avgLatencyMs: toNumberOrNull(httpReqDuration?.avg),
    p95LatencyMs: toNumberOrNull(httpReqDuration?.['p(95)']),
    errorRate: toNumberOrNull(httpReqFailed?.rate),
    requestRate: toNumberOrNull(httpReqs?.rate),
    vus: toNumberOrNull(vusMax?.value),
    checksRate: toNumberOrNull(checks?.rate),
    dataReceivedRate: toNumberOrNull(dataReceived?.rate),
    dataSentRate: toNumberOrNull(dataSent?.rate),
    acceptanceLatencyAvgMs: toNumberOrNull(orderAcceptanceLatency?.avg),
    acceptanceLatencyP95Ms: toNumberOrNull(orderAcceptanceLatency?.['p(95)']),
    terminalLatencyAvgMs: toNumberOrNull(orderTerminalLatency?.avg),
    terminalLatencyP95Ms: toNumberOrNull(orderTerminalLatency?.['p(95)']),
    terminalResolutionRate: toNumberOrNull(orderTerminalResolutionRate?.rate),
    businessSuccessRate: toNumberOrNull(orderBusinessSuccessRate?.rate),
    iterations: toNumberOrNull(iterations?.count),
    acceptedOrders: toNumberOrNull(acceptedOrders?.count),
    completedOrders: toNumberOrNull(completedOrders?.count),
    inventoryFailedOrders: toNumberOrNull(inventoryFailedOrders?.count),
    paymentFailedOrders: toNumberOrNull(paymentFailedOrders?.count),
    unresolvedOrders: toNumberOrNull(unresolvedOrders?.count)
  }
}

function createDownloads(runId: string, fileNames: string[]): RunDownload[] {
  return fileNames.map((fileName) => ({
    label: fileName,
    fileName,
    href: `/api/runs/${encodeURIComponent(runId)}/files/${encodeURIComponent(fileName)}`
  }))
}

function buildErrors(...errors: Array<string | null>): string[] {
  return errors.filter((entry): entry is string => Boolean(entry))
}

async function readLegacyRunDirectory(runDir: LegacyRunDirectory): Promise<ParsedRun | null> {
  const stat = await fs.stat(runDir.runDir).catch(() => null)
  if (!stat || !stat.isDirectory()) {
    return null
  }

  const runId = path.basename(runDir.runDir)
  const defaultTestName = path.basename(runDir.testDir)
  const metaPath = path.join(runDir.runDir, 'meta.json')
  const summaryPath = path.join(runDir.runDir, 'summary.json')
  const summaryTextPath = path.join(runDir.runDir, 'summary.txt')

  const metaResult = await safeReadJson(metaPath)
  const summaryResult = await safeReadJson(summaryPath)
  const summaryTextResult = await safeReadText(summaryTextPath)
  const summaryTextMeta = summaryTextResult.value ? parseSummaryTextMeta(summaryTextResult.value) : {}

  const meta = { ...summaryTextMeta, ...(metaResult.value ?? {}) }
  const provider = normalizeProvider(
    meta.asyncProvider ?? meta.async_provider ?? meta.syncProvider ?? meta.sync_provider,
    runId
  )
  const startedAt = parseStartedAt(meta.startedAt ?? meta.started_at, runId, stat.mtimeMs)
  const fileNames = ['meta.json', 'summary.json', 'summary.txt', 'run.log']

  return {
    id: runId,
    testName: toText(meta.testName ?? meta.test_name) || defaultTestName,
    provider,
    startedAt,
    filePath: runDir.runDir,
    meta,
    summary: summaryResult.value,
    metrics: extractMetrics(summaryResult.value),
    errors: buildErrors(metaResult.error, summaryResult.error, summaryTextResult.error),
    downloads: createDownloads(runId, fileNames),
    downloadFileNames: fileNames,
    mtimeMs: stat.mtimeMs
  }
}

function removeSummarySuffix(fileName: string): string {
  return fileName.replace(/-summary\.(json|txt)$/i, '')
}

async function readFlatRunGroup(group: FlatRunGroup): Promise<ParsedRun | null> {
  const summaryJsonName = group.files.find((fileName) => /-summary\.json$/i.test(fileName))
  if (!summaryJsonName) {
    return null
  }

  const summaryTxtName = group.files.find((fileName) => /-summary\.txt$/i.test(fileName))
  const summaryJsonPath = path.join(group.parentDir, summaryJsonName)
  const summaryTxtPath = summaryTxtName ? path.join(group.parentDir, summaryTxtName) : ''
  const stat = await fs.stat(summaryJsonPath).catch(() => null)
  if (!stat || !stat.isFile()) {
    return null
  }

  const summaryResult = await safeReadJson(summaryJsonPath)
  const summaryTextResult = summaryTxtName ? await safeReadText(summaryTxtPath) : { value: null, error: null }
  const meta = summaryTextResult.value ? parseSummaryTextMeta(summaryTextResult.value) : {}
  const runId = group.baseName
  const provider = normalizeProvider(meta.async_provider ?? meta.asyncProvider, runId, group.providerHint)
  const startedAt = parseStartedAt(meta.started_at ?? meta.startedAt, runId, stat.mtimeMs)

  return {
    id: runId,
    testName: toText(meta.test_name ?? meta.testName) || runId,
    provider,
    startedAt,
    filePath: group.parentDir,
    meta,
    summary: summaryResult.value,
    metrics: extractMetrics(summaryResult.value),
    errors: buildErrors(summaryResult.error, summaryTextResult.error),
    downloads: createDownloads(runId, [...group.files].sort()),
    downloadFileNames: [...group.files],
    mtimeMs: stat.mtimeMs
  }
}

async function discoverRunSources(resultsRoot: string): Promise<RunSource[]> {
  const entries = await fs.readdir(resultsRoot, { withFileTypes: true }).catch(() => [])
  const discovered: RunSource[] = []

  for (const entry of entries) {
    if (!entry.isDirectory()) {
      continue
    }

    const topLevelDir = path.join(resultsRoot, entry.name)
    const childEntries = await fs.readdir(topLevelDir, { withFileTypes: true }).catch(() => [])

    const flatFiles = childEntries
      .filter((child) => child.isFile() && /-summary\.(json|txt)$/i.test(child.name))
      .map((child) => child.name)

    if (flatFiles.length > 0) {
      const grouped = new Map<string, string[]>()

      for (const fileName of childEntries.filter((child) => child.isFile()).map((child) => child.name)) {
        if (!/(summary\.(json|txt)|meta\.json|run\.log)$/i.test(fileName)) {
          continue
        }

        const baseName = removeSummarySuffix(fileName)
        if (!grouped.has(baseName)) {
          grouped.set(baseName, [])
        }

        grouped.get(baseName)?.push(fileName)
      }

      for (const [baseName, files] of grouped.entries()) {
        discovered.push({
          kind: 'flat',
          parentDir: topLevelDir,
          baseName,
          files,
          providerHint: entry.name
        })
      }

      continue
    }

    for (const child of childEntries) {
      if (!child.isDirectory()) {
        continue
      }

      const runDir = path.join(topLevelDir, child.name)
      const runFiles = await fs.readdir(runDir).catch(() => [])
      if (!runFiles.includes('summary.json') && !runFiles.includes('summary.txt')) {
        continue
      }

      discovered.push({
        kind: 'legacy',
        testDir: topLevelDir,
        runDir
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
    downloads: run.downloads,
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
    requestRate: subtract(left.requestRate, right.requestRate),
    vus: subtract(left.vus, right.vus),
    checksRate: subtract(left.checksRate, right.checksRate),
    dataReceivedRate: subtract(left.dataReceivedRate, right.dataReceivedRate),
    dataSentRate: subtract(left.dataSentRate, right.dataSentRate),
    acceptanceLatencyAvgMs: subtract(left.acceptanceLatencyAvgMs, right.acceptanceLatencyAvgMs),
    acceptanceLatencyP95Ms: subtract(left.acceptanceLatencyP95Ms, right.acceptanceLatencyP95Ms),
    terminalLatencyAvgMs: subtract(left.terminalLatencyAvgMs, right.terminalLatencyAvgMs),
    terminalLatencyP95Ms: subtract(left.terminalLatencyP95Ms, right.terminalLatencyP95Ms),
    terminalResolutionRate: subtract(left.terminalResolutionRate, right.terminalResolutionRate),
    businessSuccessRate: subtract(left.businessSuccessRate, right.businessSuccessRate),
    iterations: subtract(left.iterations, right.iterations),
    acceptedOrders: subtract(left.acceptedOrders, right.acceptedOrders),
    completedOrders: subtract(left.completedOrders, right.completedOrders),
    inventoryFailedOrders: subtract(left.inventoryFailedOrders, right.inventoryFailedOrders),
    paymentFailedOrders: subtract(left.paymentFailedOrders, right.paymentFailedOrders),
    unresolvedOrders: subtract(left.unresolvedOrders, right.unresolvedOrders)
  }
}

function isInsideRoot(rootPath: string, candidatePath: string): boolean {
  const relativePath = path.relative(rootPath, candidatePath)
  return relativePath !== '' && !relativePath.startsWith('..') && !path.isAbsolute(relativePath)
}

export function createResultsStore(resultsRoot: string): ResultsStore {
  async function loadRuns(): Promise<ParsedRun[]> {
    const sources = await discoverRunSources(resultsRoot)
    const parsedRuns = await Promise.all(
      sources.map((source) => (source.kind === 'legacy' ? readLegacyRunDirectory(source) : readFlatRunGroup(source)))
    )

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
      const run = await findRun(runId)
      if (!run || !run.downloadFileNames.includes(fileName)) {
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
