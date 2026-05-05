param(
    [Parameter(Mandatory = $true)]
    [string]$ScriptPath,

    [Parameter(Mandatory = $true)]
    [string]$TestName,

    [string]$BaseUrl = 'http://order-service:8080',
    [string]$ResultsRoot = "$HOME/benchmark-results",
    [string]$CommunicationMode = '',
    [string]$SyncProvider = '',
    [string]$AsyncProvider = '',
    [string]$AzureRegion = '',
    [string]$SystemVmName = '',
    [string]$K6VmName = '',
    [string]$PayloadCount = '',
    [string]$TargetVus = '',
    [string]$RampUpDuration = '',
    [string]$SteadyDuration = '',
    [string]$RampDownDuration = '',
    [string]$GitRef = ''
)

$timestamp = Get-Date -Format 'yyyy-MM-ddTHH-mm-ss'
$providerSuffix = if ($SyncProvider) { $SyncProvider.ToLower() } elseif ($AsyncProvider) { $AsyncProvider.ToLower() } else { 'default' }
$runName = "$TestName-$providerSuffix-$timestamp"
$testDir = Join-Path $ResultsRoot $TestName
$runDir = Join-Path $testDir $runName

New-Item -ItemType Directory -Force -Path $runDir | Out-Null

$meta = @{
    testName = $TestName
    runName = $runName
    startedAt = (Get-Date).ToString('o')
    azureRegion = $AzureRegion
    systemVmName = $SystemVmName
    k6VmName = $K6VmName
    baseUrl = $BaseUrl
    communicationMode = $CommunicationMode
    syncProvider = $SyncProvider
    asyncProvider = $AsyncProvider
    payloadCount = $PayloadCount
    targetVus = $TargetVus
    rampUpDuration = $RampUpDuration
    steadyDuration = $SteadyDuration
    rampDownDuration = $RampDownDuration
    scriptPath = $ScriptPath
    gitRef = $GitRef
} | ConvertTo-Json -Depth 5

$meta | Out-File -FilePath (Join-Path $runDir 'meta.json') -Encoding utf8

$env:RESULTS_DIR = $testDir
$env:RUN_NAME = $runName
$env:BASE_URL = $BaseUrl
$env:COMMUNICATION_MODE = $CommunicationMode
$env:SYNC_PROVIDER = $SyncProvider
$env:ASYNC_PROVIDER = $AsyncProvider
$env:AZURE_REGION = $AzureRegion
$env:SYSTEM_VM_NAME = $SystemVmName
$env:K6_VM_NAME = $K6VmName
$env:PAYLOAD_COUNT = $PayloadCount
$env:TARGET_VUS = $TargetVus
$env:RAMP_UP_DURATION = $RampUpDuration
$env:STEADY_DURATION = $SteadyDuration
$env:RAMP_DOWN_DURATION = $RampDownDuration
$env:SCRIPT_PATH = $ScriptPath
$env:GIT_REF = $GitRef

k6 run $ScriptPath 2>&1 | Tee-Object -FilePath (Join-Path $runDir 'run.log')
exit $LASTEXITCODE
