import { useEffect, useRef, useState } from 'react'
import { fetchComparison, fetchRun, fetchRuns } from './api'
import { RunDetail } from './components/RunDetail'
import { RunList } from './components/RunList'
import { RunComparison } from './components/RunComparison'
import type { RunComparison as RunComparisonData, RunSummary } from './types'

export default function App() {
  const comparisonRequestId = useRef(0)
  const [runs, setRuns] = useState<RunSummary[]>([])
  const [search, setSearch] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [selectedRun, setSelectedRun] = useState<RunSummary | null>(null)
  const [comparison, setComparison] = useState<RunComparisonData | null>(null)
  const [compareLeftId, setCompareLeftId] = useState<string | null>(null)
  const [compareRightId, setCompareRightId] = useState<string | null>(null)
  const [compareLeftRun, setCompareLeftRun] = useState<RunSummary | null>(null)
  const [compareRightRun, setCompareRightRun] = useState<RunSummary | null>(null)
  const [isRunsLoading, setIsRunsLoading] = useState(false)
  const [isRunLoading, setIsRunLoading] = useState(false)
  const [isComparisonLoading, setIsComparisonLoading] = useState(false)
  const [runsError, setRunsError] = useState<string | null>(null)
  const [selectedRunError, setSelectedRunError] = useState<string | null>(null)
  const [comparisonError, setComparisonError] = useState<string | null>(null)

  const resetComparison = () => {
    comparisonRequestId.current += 1
    setComparison(null)
    setComparisonError(null)
    setIsComparisonLoading(false)
  }

  const loadComparison = () => {
    if (!compareLeftId || !compareRightId) {
      return
    }

    const requestId = comparisonRequestId.current + 1
    comparisonRequestId.current = requestId
    setIsComparisonLoading(true)
    setComparisonError(null)

    fetchComparison(compareLeftId, compareRightId)
      .then((nextComparison) => {
        if (comparisonRequestId.current !== requestId) {
          return
        }

        setComparison(nextComparison)
      })
      .catch((error: Error) => {
        if (comparisonRequestId.current !== requestId) {
          return
        }

        setComparison(null)
        setComparisonError(error.message)
      })
      .finally(() => {
        if (comparisonRequestId.current !== requestId) {
          return
        }

        setIsComparisonLoading(false)
      })
  }

  useEffect(() => {
    let isActive = true

    setIsRunsLoading(true)

    fetchRuns(search)
      .then((nextRuns) => {
        if (!isActive) {
          return
        }

        setRuns(nextRuns)
        setRunsError(null)
        setCompareLeftRun((currentRun) =>
          currentRun ? nextRuns.find((run) => run.id === currentRun.id) ?? currentRun : null
        )
        setCompareRightRun((currentRun) =>
          currentRun ? nextRuns.find((run) => run.id === currentRun.id) ?? currentRun : null
        )
        setSelectedId((currentSelectedId) => {
          if (nextRuns.length === 0) {
            return null
          }

          if (currentSelectedId && nextRuns.some((run) => run.id === currentSelectedId)) {
            return currentSelectedId
          }

          return nextRuns[0].id
        })
      })
      .catch((error: Error) => {
        if (!isActive) {
          return
        }

        setRuns([])
        setSelectedId(null)
        setSelectedRun(null)
        resetComparison()
        setRunsError(error.message)
      })
      .finally(() => {
        if (isActive) {
          setIsRunsLoading(false)
        }
      })

    return () => {
      isActive = false
    }
  }, [search])

  useEffect(() => {
    let isActive = true

    if (!selectedId) {
      setSelectedRun(null)
      setSelectedRunError(null)
      setIsRunLoading(false)
      return () => {
        isActive = false
      }
    }

    setIsRunLoading(true)

    fetchRun(selectedId)
      .then((run) => {
        if (!isActive) {
          return
        }

        setSelectedRun(run)
        setSelectedRunError(null)
      })
      .catch((error: Error) => {
        if (!isActive) {
          return
        }

        setSelectedRun(null)
        setSelectedRunError(error.message)
      })
      .finally(() => {
        if (isActive) {
          setIsRunLoading(false)
        }
      })

    return () => {
      isActive = false
    }
  }, [selectedId])

  return (
    <div className="app-shell">
      <RunList
        runs={runs}
        search={search}
        selectedId={selectedId}
        compareLeftId={compareLeftId}
        compareRightId={compareRightId}
        isLoading={isRunsLoading}
        error={runsError}
        onSearchChange={setSearch}
        onSelectRun={setSelectedId}
        onToggleCompareLeft={(id) => {
          const nextId = compareLeftId === id ? null : id
          resetComparison()
          setCompareLeftId(nextId)
          setCompareLeftRun(nextId ? runs.find((run) => run.id === nextId) ?? compareLeftRun : null)
        }}
        onToggleCompareRight={(id) => {
          const nextId = compareRightId === id ? null : id
          resetComparison()
          setCompareRightId(nextId)
          setCompareRightRun(nextId ? runs.find((run) => run.id === nextId) ?? compareRightRun : null)
        }}
      />

      <main className="content-panel">
        <RunDetail
          run={selectedRun}
          isLoading={isRunLoading}
          error={selectedRunError}
          isCompareLeft={selectedRun?.id === compareLeftId}
          isCompareRight={selectedRun?.id === compareRightId}
        />
        <RunComparison
          comparison={comparison}
          compareLeftRun={compareLeftRun}
          compareRightRun={compareRightRun}
          isLoading={isComparisonLoading}
          error={comparisonError}
          onCompare={loadComparison}
        />
      </main>
    </div>
  )
}
