import os from 'os'
import path from 'path'
import { createApp } from './app'
import { createResultsStore } from './resultsStore'

const port = Number(process.env.PORT ?? '4173') || 4173
const homeDir = process.env.HOME ?? os.homedir()
const resultsRoot = process.env.RESULTS_ROOT ?? path.join(homeDir, 'benchmark-results')

const store = createResultsStore(resultsRoot)
const app = createApp(store)

app.listen(port, () => {
  console.log(`Reading results from ${resultsRoot}`)
  console.log(`Benchmark Results Viewer listening on port ${port}`)
})
