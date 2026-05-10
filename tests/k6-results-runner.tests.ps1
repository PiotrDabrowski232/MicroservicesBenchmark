$helperPath = Join-Path $PSScriptRoot '..\docker\lib\results.js'
$runnerPath = Join-Path $PSScriptRoot '..\scripts\run-k6-with-results.ps1'

if (-not (Test-Path $helperPath)) {
    throw 'results.js not found'
}

if (-not (Test-Path $runnerPath)) {
    throw 'run-k6-with-results.ps1 not found'
}

Write-Host 'Runner and helper paths exist.'
