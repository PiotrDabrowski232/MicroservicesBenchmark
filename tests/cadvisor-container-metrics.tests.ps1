$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$composePath = Join-Path $repoRoot 'docker\docker-compose.yml'
$composeContent = Get-Content $composePath -Raw

if ($composeContent -notmatch 'image:\s*ghcr\.io/google/cadvisor:(latest|v0\.56\.[0-9]+)') {
    throw 'cAdvisor image must use ghcr.io/google/cadvisor with a current v0.56.x or latest tag.'
}

$requiredSnippets = @(
    '- /var/run/docker.sock:/var/run/docker.sock:ro',
    '- /sys/fs/cgroup:/sys/fs/cgroup:ro',
    '--docker_only=true'
)

foreach ($snippet in $requiredSnippets) {
    if ($composeContent -notmatch [regex]::Escape($snippet)) {
        throw "Missing cAdvisor configuration: $snippet"
    }
}

Write-Host 'cAdvisor container metrics configuration is present.'
