import { useEffect, useState } from 'react'
import { fetchRun, fetchRuns } from './api'
import { RunDetail } from './components/RunDetail'
import { RunList } from './components/RunList'
import type { RunSummary } from './types'

export default function App() {
  const [runs, setRuns] = useState<RunSummary[]>([])
  const [search, setSearch] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [selectedRun, setSelectedRun] = useState<RunSummary | null>(null)
  const [compareLeftId, setCompareLeftId] = useState<string | null>(null)
  const [compareRightId, setCompareRightId] = useState<string | null>(null)
  const [isRunsLoading, setIsRunsLoading] = useState(false)
  const [isRunLoading, setIsRunLoading] = useState(false)
  const [runsError, setRunsError] = useState<string | null>(null)
  const [selectedRunError, setSelectedRunError] = useState<string | null>(null)

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
          setCompareLeftId((currentValue) => (currentValue === id ? null : id))
        }}
        onToggleCompareRight={(id) => {
          setCompareRightId((currentValue) => (currentValue === id ? null : id))
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
      </main>
    </div>
  )
}
