$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

$requiredFiles = @(
    'docker\grafana\provisioning\dashboards\dashboards.yml',
    'docker\grafana\dashboards\request-performance.json',
    'docker\grafana\dashboards\container-resource-usage.json',
    'docker\grafana\dashboards\test-run-overview.json'
)

foreach ($relativePath in $requiredFiles) {
    $fullPath = Join-Path $repoRoot $relativePath
    if (-not (Test-Path $fullPath)) {
        throw "Missing required file: $relativePath"
    }
}

$requestDashboard = Get-Content (Join-Path $repoRoot 'docker\grafana\dashboards\request-performance.json') -Raw | ConvertFrom-Json
$resourceDashboard = Get-Content (Join-Path $repoRoot 'docker\grafana\dashboards\container-resource-usage.json') -Raw | ConvertFrom-Json
$overviewDashboard = Get-Content (Join-Path $repoRoot 'docker\grafana\dashboards\test-run-overview.json') -Raw | ConvertFrom-Json

if ($requestDashboard.title -ne 'Request Performance') {
    throw "Unexpected request dashboard title: $($requestDashboard.title)"
}

if ($resourceDashboard.title -ne 'Container Resource Usage') {
    throw "Unexpected resource dashboard title: $($resourceDashboard.title)"
}

if ($overviewDashboard.title -ne 'Test Run Overview') {
    throw "Unexpected overview dashboard title: $($overviewDashboard.title)"
}

$providerConfig = Get-Content (Join-Path $repoRoot 'docker\grafana\provisioning\dashboards\dashboards.yml') -Raw

if ($providerConfig -notmatch 'path:\s*/var/lib/grafana/dashboards') {
    throw 'Dashboard provider does not point to /var/lib/grafana/dashboards.'
}

$requestPanelTitles = @($requestDashboard.panels | ForEach-Object { $_.title })
$expectedRequestPanels = @(
    'Throughput per Service',
    'Average Latency per Service',
    'P95 Latency per Service',
    '5xx Error Rate',
    'Total Requests in Range'
)

foreach ($panelTitle in $expectedRequestPanels) {
    if ($requestPanelTitles -notcontains $panelTitle) {
        throw "Missing request dashboard panel: $panelTitle"
    }
}

Write-Host 'Grafana dashboard provisioning files are present.'
