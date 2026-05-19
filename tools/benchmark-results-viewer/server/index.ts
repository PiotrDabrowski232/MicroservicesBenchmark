import os from 'os'
import path from 'path'
import { existsSync } from 'fs'
import { createApp } from './app'
import { createResultsStore } from './resultsStore'

const port = Number(process.env.PORT ?? '4173') || 4173
const homeDir = process.env.HOME ?? os.homedir()

function looksLikeBrokerResultsRoot(candidatePath: string) {
  return ['RabbitMQResults', 'KafkaResults', 'LavinResults'].some((directoryName) =>
    existsSync(path.join(candidatePath, directoryName))
  )
}

function resolveResultsRoot() {
  if (process.env.RESULTS_ROOT) {
    return process.env.RESULTS_ROOT
  }

  const currentWorkingDirectory = process.cwd()
  if (looksLikeBrokerResultsRoot(currentWorkingDirectory)) {
    return currentWorkingDirectory
  }

  const repositoryRoot = path.resolve(currentWorkingDirectory, '..', '..')
  if (looksLikeBrokerResultsRoot(repositoryRoot)) {
    return repositoryRoot
  }

  return path.join(homeDir, 'benchmark-results')
}

const resultsRoot = resolveResultsRoot()

const store = createResultsStore(resultsRoot)
const app = createApp(store)

app.listen(port, () => {
  console.log(`Reading results from ${resultsRoot}`)
  console.log(`Benchmark Results Viewer listening on port ${port}`)
})
