param(
    [string]$Environment = "Development",
    [string]$Urls = "http://localhost:5050"
)

$ErrorActionPreference = "Stop"

$apiDir = Join-Path $PSScriptRoot "..\..\backend\src\HomeChef.Api"
$apiDir = [System.IO.Path]::GetFullPath($apiDir)

$existing = Get-NetTCPConnection -LocalPort 5050 -State Listen -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "API is already listening on $Urls (PID $($existing[0].OwningProcess))."
    exit 0
}

$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:ASPNETCORE_URLS = $Urls

Write-Host "Starting HomeChef API on $Urls ..."
Start-Process -FilePath "dotnet" `
    -ArgumentList 'run', '--project', "`"$apiDir`"", '--no-launch-profile' `
    -WorkingDirectory $apiDir -WindowStyle Hidden | Out-Null

Write-Host "API starting in background. Health check: $Urls/health"