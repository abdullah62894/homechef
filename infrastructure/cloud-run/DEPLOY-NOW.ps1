# HomeChef API - Google Cloud Run Deployment Script
# Deploys API to Google Cloud Run with Neon PostgreSQL

param(
    [Parameter(Mandatory=$false)]
    [string]$ProjectId = $env:GCP_PROJECT_ID,
    
    [Parameter(Mandatory=$false)]
    [string]$Region = "us-central1",

    [Parameter(Mandatory=$false)]
    [string]$NeonDbUrl = $env:NEON_DB_URL,

    [Parameter(Mandatory=$false)]
    [string]$JwtSigningKey = $env:JWT_SIGNING_KEY,

    [Parameter(Mandatory=$false)]
    [string]$AdminSeedEmail = $env:ADMIN_SEED_EMAIL,

    [Parameter(Mandatory=$false)]
    [string]$AdminSeedPassword = $env:ADMIN_SEED_PASSWORD,

    [Parameter(Mandatory=$false)]
    [string]$CorsAllowedOrigin = $env:CORS_ALLOWED_ORIGIN
)

$ErrorActionPreference = "Continue"

if (-not $ProjectId) {
    $ProjectId = gcloud config get-value project 2>$null
    if (-not $ProjectId) {
        Write-Error "Please provide -ProjectId or run 'gcloud config set project <your-project-id>'."
        exit 1
    }
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  HomeChef API - Google Cloud Run Deploy" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Project: $ProjectId" -ForegroundColor Yellow
Write-Host "Region:  $Region" -ForegroundColor Yellow
Write-Host "Service: homechef-api" -ForegroundColor Yellow
Write-Host ""

Write-Host "Step 1: Setting project..." -ForegroundColor Green
gcloud config set project $ProjectId

Write-Host "Step 2: Enabling required APIs..." -ForegroundColor Green
gcloud services enable run.googleapis.com containerregistry.googleapis.com cloudbuild.googleapis.com secretmanager.googleapis.com

Write-Host "Step 3: Checking secrets..." -ForegroundColor Green

$secretsToSet = @()
if ($NeonDbUrl) { $secretsToSet += @{ Name = "connection-string"; Value = $NeonDbUrl } }
if ($JwtSigningKey) { $secretsToSet += @{ Name = "jwt-signing-key"; Value = $JwtSigningKey } }
if ($AdminSeedEmail) { $secretsToSet += @{ Name = "admin-seed-email"; Value = $AdminSeedEmail } }
if ($AdminSeedPassword) { $secretsToSet += @{ Name = "admin-seed-password"; Value = $AdminSeedPassword } }
if ($CorsAllowedOrigin) { $secretsToSet += @{ Name = "cors-allowed-origins"; Value = $CorsAllowedOrigin } }

foreach ($secret in $secretsToSet) {
    Write-Host "  Ensuring secret: $($secret.Name)" -ForegroundColor Gray
    $exists = gcloud secrets describe $secret.Name 2>&1
    if ($LASTEXITCODE -ne 0) {
        $secret.Value | gcloud secrets create $secret.Name --data-file=- --quiet
    } else {
        $secret.Value | gcloud secrets versions add $secret.Name --data-file=- --quiet
    }
}

# Ensure root Dockerfile exists for Cloud Build source deploy
if (-not (Test-Path "Dockerfile")) {
    Copy-Item "infrastructure/docker/Dockerfile.api" "Dockerfile"
}

Write-Host "Step 4: Building and deploying (this may take 5-10 minutes)..." -ForegroundColor Green
Write-Host ""

# Deploy directly using source build (no Docker needed locally)
gcloud run deploy homechef-api `
    --source . `
    --region $Region `
    --platform managed `
    --allow-unauthenticated `
    --memory 256Mi `
    --cpu 1 `
    --min-instances 0 `
    --max-instances 2 `
    --port 8080 `
    --set-env-vars "ASPNETCORE_ENVIRONMENT=Production,DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false,DOTNET_USE_POLLING_FILE_WATCHER=true,DOTNET_EnableDiagnostics=0,Database__AutoMigrate=true,AWS_ENDPOINT_URL_S3=https://br-delicate-salad-a5lwkpmi.storage.c-1.us-east-2.aws.neon.tech,AWS_REGION=us-east-2,NEON_STORAGE_BUCKET=homechef" `
    --set-secrets "ConnectionStrings__Default=connection-string:latest,Jwt__SigningKey=jwt-signing-key:latest,Admin__SeedAdminEmail=admin-seed-email:latest,Admin__SeedAdminPassword=admin-seed-password:latest,Cors__AllowedOrigins__0=cors-allowed-origins:latest,AWS_ACCESS_KEY_ID=neon-s3-access-key-id:latest,AWS_SECRET_ACCESS_KEY=neon-s3-secret-access-key:latest"

# Get the service URL
$serviceUrl = gcloud run services describe homechef-api --region $Region --format="value(status.url)"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  DEPLOYMENT COMPLETE!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "API URL: $serviceUrl" -ForegroundColor Yellow
Write-Host ""
Write-Host "Test your API:" -ForegroundColor White
Write-Host "  curl $serviceUrl/health" -ForegroundColor Gray
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor White
Write-Host "  1. Update your frontend NEXT_PUBLIC_API_URL to: $serviceUrl" -ForegroundColor Gray
Write-Host "  2. Test all API endpoints" -ForegroundColor Gray
Write-Host "  3. Update CORS if needed" -ForegroundColor Gray
