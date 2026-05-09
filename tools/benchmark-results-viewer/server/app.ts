import express from 'express'
import { existsSync } from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'
import type { ResultsStore } from './types'

function readString(value: unknown): string | undefined {
  return typeof value === 'string' && value.trim() !== '' ? value.trim() : undefined
}

export function createApp(store: ResultsStore) {
  const app = express()
  const serverDir = path.dirname(fileURLToPath(import.meta.url))
  const distDir = path.resolve(serverDir, '..', 'dist')
  const indexHtml = path.join(distDir, 'index.html')

  app.use(express.json())
  app.use(express.static(distDir))

  app.get('/api/runs/compare', async (req, res, next) => {
    try {
      const left = readString(req.query.left)
      const right = readString(req.query.right)

      if (!left || !right) {
        res.status(400).json({ error: 'Query parameters "left" and "right" are required.' })
        return
      }

      const comparison = await store.compareRuns(left, right)
      if (!comparison) {
        res.status(404).json({ error: 'One or both runs were not found.' })
        return
      }

      res.json(comparison)
    } catch (error) {
      next(error)
    }
  })

  app.get('/api/runs', async (req, res, next) => {
    try {
      const runs = await store.listRuns({
        testName: readString(req.query.testName),
        provider: readString(req.query.provider),
        search: readString(req.query.search)
      })

      res.json(runs)
    } catch (error) {
      next(error)
    }
  })

  app.get('/api/runs/:id/files/:name', async (req, res, next) => {
    try {
      const filePath = await store.getDownloadPath(req.params.id, req.params.name)
      if (!filePath) {
        res.status(404).json({ error: 'File not found.' })
        return
      }

      const downloadName = path.basename(filePath)
      const contentType =
        downloadName.endsWith('.json') ? 'application/json; charset=utf-8' : 'text/plain; charset=utf-8'

      res.setHeader('Content-Disposition', `attachment; filename="${downloadName}"`)
      res.type(contentType)
      res.sendFile(filePath)
    } catch (error) {
      next(error)
    }
  })

  app.get('/api/runs/:id', async (req, res, next) => {
    try {
      const run = await store.getRun(req.params.id)
      if (!run) {
        res.status(404).json({ error: 'Run not found.' })
        return
      }

      res.json(run)
    } catch (error) {
      next(error)
    }
  })

  app.get(/^(?!\/api\/).*/, (_req, res, next) => {
    if (!existsSync(indexHtml)) {
      res.status(200).json({ message: 'Frontend not built yet.' })
      return
    }

    res.sendFile(indexHtml, (error) => {
      if (error) {
        next(error)
      }
    })
  })

  app.use((error: unknown, _req, res, _next) => {
    const message = error instanceof Error ? error.message : 'Unexpected server error.'
    res.status(500).json({ error: message })
  })

  return app
}
