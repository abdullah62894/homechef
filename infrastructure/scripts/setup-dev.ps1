param(
    [int]$Port = 5433,
    [string]$SuperUser = "postgres",
    [string]$SuperPassword = "homechef_dev_pw"
)

$ErrorActionPreference = "Stop"

$PgBin = "C:\Program Files\PostgreSQL\18\bin"
$PgData = "C:\Users\SAAD\AppData\Local\PostgreSQL\18\data"
$PgLog = "C:\Users\SAAD\AppData\Local\PostgreSQL\18\server.log"

function Test-PostgresRunning {
    & "$PgBin\pg_isready.exe" -h 127.0.0.1 -p $Port 2>$null
    return ($LASTEXITCODE -eq 0)
}

if (-not (Test-Path "$PgBin\pg_ctl.exe")) {
    throw "PostgreSQL was not found at $PgBin. Install it first (winget install PostgreSQL.PostgreSQL.18)."
}

if (-not (Test-Path "$PgData\PG_VERSION")) {
    Write-Host "Initializing PostgreSQL cluster at $PgData ..."
    $pwFile = Join-Path $env:TEMP "homechef-pgpw.txt"
    Set-Content -Path $pwFile -Value $SuperPassword -NoNewline
    & "$PgBin\initdb.exe" -D $PgData -U $SuperUser -A scram-sha-256 --pwfile=$pwFile -E UTF8 --locale=C
    Remove-Item $pwFile -Force

    $conf = Join-Path $PgData "postgresql.conf"
    (Get-Content $conf) -replace '^#?port\s*=\s*\d+', "port = $Port" | Set-Content $conf
}

if (-not (Test-PostgresRunning)) {
    Write-Host "Starting PostgreSQL on port $Port ..."
    Start-Process -FilePath "$PgBin\pg_ctl.exe" `
        -ArgumentList '-D', "`"$PgData`"", '-l', "`"$PgLog`"", '-w', 'start' `
        -WindowStyle Hidden | Out-Null
    Start-Sleep -Seconds 4
    if (-not (Test-PostgresRunning)) {
        throw "PostgreSQL failed to start. Check $PgLog"
    }
}

$env:PGPASSWORD = $SuperPassword
foreach ($db in @("homechef", "homechef_test")) {
    $exists = & "$PgBin\psql.exe" -h 127.0.0.1 -p $Port -U $SuperUser -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$db';"
    if ($exists.Trim() -ne "1") {
        Write-Host "Creating database '$db' ..."
        & "$PgBin\psql.exe" -h 127.0.0.1 -p $Port -U $SuperUser -d postgres -c "CREATE DATABASE $db;"
    }
}

Write-Host ""
Write-Host "PostgreSQL is ready:"
Write-Host "  Host: 127.0.0.1"
Write-Host "  Port: $Port"
Write-Host "  User: $SuperUser"
Write-Host "  DBs:  homechef, homechef_test"
Write-Host ""
Write-Host "Connection string: Host=127.0.0.1;Port=$Port;Database=homechef;Username=$SuperUser;Password=$SuperPassword"