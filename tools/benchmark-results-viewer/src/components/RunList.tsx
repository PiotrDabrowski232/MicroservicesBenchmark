import type { RunSummary } from '../types'
import { formatDateTime } from '../utils/date'

type RunListProps = {
  runs: RunSummary[]
  search: string
  selectedId: string | null
  compareLeftId: string | null
  compareRightId: string | null
  isLoading: boolean
  error: string | null
  onSearchChange: (value: string) => void
  onSelectRun: (id: string) => void
  onToggleCompareLeft: (id: string) => void
  onToggleCompareRight: (id: string) => void
}

export function RunList({
  runs,
  search,
  selectedId,
  compareLeftId,
  compareRightId,
  isLoading,
  error,
  onSearchChange,
  onSelectRun,
  onToggleCompareLeft,
  onToggleCompareRight
}: RunListProps) {
  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <p className="eyebrow">Internal tool</p>
        <h1>Benchmark Results Viewer</h1>
      </div>

      <label className="search-field">
        <span className="search-label">Search runs</span>
        <input
          type="search"
          value={search}
          placeholder="Search by run ID, test or provider"
          onChange={(event) => onSearchChange(event.target.value)}
        />
      </label>

      {error ? <div className="panel-message panel-message-error">{error}</div> : null}
      {isLoading && runs.length === 0 ? <div className="panel-message">Loading runs…</div> : null}
      {!error && !isLoading && runs.length === 0 ? (
        <div className="panel-message">No benchmark runs found.</div>
      ) : null}

      <div className="run-list">
        {runs.map((run) => {
          const isSelected = run.id === selectedId
          const isCompareLeft = run.id === compareLeftId
          const isCompareRight = run.id === compareRightId

          return (
            <div className={`run-list-item${isSelected ? ' is-selected' : ''}`} key={run.id}>
              <button
                type="button"
                className="run-list-button"
                onClick={() => onSelectRun(run.id)}
              >
                <span className="run-list-title">{run.id}</span>
                <span className="run-list-subtitle">
                  {run.provider.toUpperCase()} · {run.testName}
                </span>
                <span className="run-list-meta">
                  {formatDateTime(run.startedAt, {
                    dateStyle: 'medium',
                    timeStyle: 'short'
                  })}
                </span>
              </button>

              <div className="compare-actions">
                <button
                  type="button"
                  className={`compare-toggle${isCompareLeft ? ' is-active' : ''}`}
                  aria-pressed={isCompareLeft}
                  onClick={() => onToggleCompareLeft(run.id)}
                >
                  {isCompareLeft ? 'A selected' : 'Set A'}
                </button>
                <button
                  type="button"
                  className={`compare-toggle${isCompareRight ? ' is-active' : ''}`}
                  aria-pressed={isCompareRight}
                  onClick={() => onToggleCompareRight(run.id)}
                >
                  {isCompareRight ? 'B selected' : 'Set B'}
                </button>
              </div>
            </div>
          )
        })}
      </div>
    </aside>
  )
}
