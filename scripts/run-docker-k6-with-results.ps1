param(
    [Parameter(Mandatory = $true)]
    [string]$ScriptPath,

    [string]$TestName = '',
    [string]$BaseUrl = 'http://host.docker.internal:5001',
    [string]$ResultsRoot = '',
    [string]$AsyncProvider = '',
    [string]$CommunicationMode = '',
    [string]$SyncProvider = '',
    [string]$AzureRegion = '',
    [string]$SystemVmName = '',
    [string]$K6VmName = '',
    [string]$PayloadCount = '',
    [string]$TargetVus = '',
    [string]$OrderCompletionTimeoutMs = '',
    [string]$OrderStatusPollIntervalMs = '',
    [string]$RampUpDuration = '',
    [string]$SteadyDuration = '',
    [string]$RampDownDuration = '',
    [string]$GitRef = ''
)

$ErrorActionPreference = 'Stop'

function Get-EnvValue {
    param(
        [string]$EnvFilePath,
        [string]$Key
    )

    if (-not (Test-Path $EnvFilePath)) {
        return ''
    }

    $pattern = "^\s*$([regex]::Escape($Key))\s*=\s*(.*)\s*$"

    foreach ($line in Get-Content $EnvFilePath) {
        if ($line -match '^\s*#' -or [string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line -match $pattern) {
            return $Matches[1].Trim()
        }
    }

    return ''
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$envFilePath = Join-Path $repoRoot 'docker\.env'

if ([string]::IsNullOrWhiteSpace($TestName)) {
    $TestName = [System.IO.Path]::GetFileNameWithoutExtension($ScriptPath)
}

if ([string]::IsNullOrWhiteSpace($AsyncProvider)) {
    $AsyncProvider = Get-EnvValue -EnvFilePath $envFilePath -Key 'ASYNC_PROVIDER'
}

if ([string]::IsNullOrWhiteSpace($CommunicationMode)) {
    $CommunicationMode = Get-EnvValue -EnvFilePath $envFilePath -Key 'COMMUNICATION_MODE'
}

if ([string]::IsNullOrWhiteSpace($SyncProvider)) {
    $SyncProvider = Get-EnvValue -EnvFilePath $envFilePath -Key 'SYNC_PROVIDER'
}

$providerLabel = switch ($AsyncProvider.ToLowerInvariant()) {
    'rabbitmq' { 'RabbitMQResults' }
    'kafka' { 'KafkaResults' }
    'lavinmq' { 'LavinResults' }
    default { 'Results' }
}

if ([string]::IsNullOrWhiteSpace($ResultsRoot)) {
    $ResultsRoot = Join-Path $repoRoot $providerLabel
}

$providerSuffix = if ($AsyncProvider) { $AsyncProvider.ToLowerInvariant() } else { 'default' }
$timestamp = Get-Date -Format 'yyyy-MM-ddTHH-mm-ss'
$runName = "$TestName-$providerSuffix-$timestamp"
$runDir = Join-Path $ResultsRoot $runName

New-Item -ItemType Directory -Force -Path $runDir | Out-Null

$meta = @{
    testName = $TestName
    runName = $runName
    startedAt = (Get-Date).ToString('o')
    baseUrl = $BaseUrl
    communicationMode = $CommunicationMode
    syncProvider = $SyncProvider
    asyncProvider = $AsyncProvider
    azureRegion = $AzureRegion
    systemVmName = $SystemVmName
    k6VmName = $K6VmName
    payloadCount = $PayloadCount
    targetVus = $TargetVus
    orderCompletionTimeoutMs = $OrderCompletionTimeoutMs
    orderStatusPollIntervalMs = $OrderStatusPollIntervalMs
    rampUpDuration = $RampUpDuration
    steadyDuration = $SteadyDuration
    rampDownDuration = $RampDownDuration
    scriptPath = $ScriptPath
    gitRef = $GitRef
} | ConvertTo-Json -Depth 5

$meta | Out-File -FilePath (Join-Path $runDir 'meta.json') -Encoding utf8

$relativeScriptPath = $ScriptPath.Replace('\', '/')
$relativeResultsDir = [System.IO.Path]::GetRelativePath($repoRoot, $ResultsRoot).Replace('\', '/')
$logPath = Join-Path $runDir "$TestName-run.log"

$dockerArgs = @(
    'run', '--rm', '-i',
    '-v', "${repoRoot}:/work",
    '-w', '/work',
    '-e', "BASE_URL=$BaseUrl",
    '-e', "RESULTS_DIR=$relativeResultsDir",
    '-e', "RUN_NAME=$runName",
    '-e', "COMMUNICATION_MODE=$CommunicationMode",
    '-e', "SYNC_PROVIDER=$SyncProvider",
    '-e', "ASYNC_PROVIDER=$AsyncProvider",
    '-e', "AZURE_REGION=$AzureRegion",
    '-e', "SYSTEM_VM_NAME=$SystemVmName",
    '-e', "K6_VM_NAME=$K6VmName",
    '-e', "PAYLOAD_COUNT=$PayloadCount",
    '-e', "TARGET_VUS=$TargetVus",
    '-e', "ORDER_COMPLETION_TIMEOUT_MS=$OrderCompletionTimeoutMs",
    '-e', "ORDER_STATUS_POLL_INTERVAL_MS=$OrderStatusPollIntervalMs",
    '-e', "RAMP_UP_DURATION=$RampUpDuration",
    '-e', "STEADY_DURATION=$SteadyDuration",
    '-e', "RAMP_DOWN_DURATION=$RampDownDuration",
    '-e', "SCRIPT_PATH=$relativeScriptPath",
    '-e', "GIT_REF=$GitRef",
    'grafana/k6',
    'run',
    $relativeScriptPath
)

& docker @dockerArgs 2>&1 | Tee-Object -FilePath $logPath
exit $LASTEXITCODE
