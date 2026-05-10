import type { RunComparison, RunSummary } from './types'

async function requestJson<T>(path: string): Promise<T> {
  const response = await fetch(path)

  if (!response.ok) {
    let message = `Request failed with status ${response.status}.`

    try {
      const payload = (await response.json()) as { error?: string }
      if (payload.error) {
        message = payload.error
      }
    } catch {
      // Ignore invalid JSON responses and use the fallback message.
    }

    throw new Error(message)
  }

  return response.json() as Promise<T>
}

export function fetchRuns(search = '') {
  const params = new URLSearchParams()

  if (search.trim() !== '') {
    params.set('search', search.trim())
  }

  const query = params.toString()
  return requestJson<RunSummary[]>(`/api/runs${query ? `?${query}` : ''}`)
}

export function fetchRun(id: string) {
  return requestJson<RunSummary>(`/api/runs/${encodeURIComponent(id)}`)
}

export function fetchComparison(left: string, right: string) {
  const params = new URLSearchParams({
    left,
    right
  })

  return requestJson<RunComparison>(`/api/runs/compare?${params.toString()}`)
}
