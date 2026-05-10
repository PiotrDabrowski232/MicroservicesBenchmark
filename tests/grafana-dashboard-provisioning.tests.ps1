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

$providerConfig = Get-Content (Join-Path $repoRoot 'docker\grafana\provisioning\dashboards\dashboards.yml') -Raw

if ($providerConfig -notmatch 'path:\s*/var/lib/grafana/dashboards') {
    throw 'Dashboard provider does not point to /var/lib/grafana/dashboards.'
}

$expectedTitles = @{
    'Request Performance' = @(
        'Throughput per Service',
        'Average Latency per Service',
        'P95 Latency per Service',
        '5xx Error Rate',
        'Total Requests in Range'
    )
    'Container Resource Usage' = @(
        'CPU Usage per Service',
        'Memory Usage per Service',
        'Network Receive per Service',
        'Network Transmit per Service'
    )
    'Test Run Overview' = @(
        'Prometheus Target Health',
        'Benchmark Service Availability',
        'Observed Benchmark Containers',
        'Recent Request Rate'
    )
}

$dashboards = @($requestDashboard, $resourceDashboard, $overviewDashboard)

foreach ($dashboard in $dashboards) {
    if (-not $expectedTitles.ContainsKey($dashboard.title)) {
        throw "Unexpected dashboard title: $($dashboard.title)"
    }

    $panelTitles = @($dashboard.panels | ForEach-Object { $_.title })

    foreach ($panelTitle in $expectedTitles[$dashboard.title]) {
        if ($panelTitles -notcontains $panelTitle) {
            throw "Missing panel '$panelTitle' in dashboard '$($dashboard.title)'"
        }
    }
}

Write-Host 'Grafana dashboard provisioning files are present.'
