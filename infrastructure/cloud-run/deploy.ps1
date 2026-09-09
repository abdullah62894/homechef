# HomeChef API - Google Cloud Run Deployment Script (PowerShell)
# This script deploys the API to Google Cloud Run with Neon DB

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
        Write-Error "Please specify -ProjectId or run 'gcloud config set project <id>'."
        exit 1
    }
}

Write-Host "=== HomeChef API Deployment to Google Cloud Run ===" -ForegroundColor Green
Write-Host ""
Write-Host "Project: $ProjectId" -ForegroundColor Cyan
Write-Host "Region: $Region" -ForegroundColor Cyan
Write-Host "Service: homechef-api" -ForegroundColor Cyan
Write-Host ""

# Step 1: Enable required APIs
Write-Host "Step 1: Enabling required Google Cloud APIs..." -ForegroundColor Yellow
gcloud services enable run.googleapis.com
gcloud services enable containerregistry.googleapis.com
gcloud services enable cloudbuild.googleapis.com
gcloud services enable secretmanager.googleapis.com

# Step 2: Build and push Docker image
Write-Host "Step 2: Building Docker image..." -ForegroundColor Yellow
docker build -t "gcr.io/$ProjectId/homechef-api:latest" -f "infrastructure/docker/Dockerfile.api" .

Write-Host "Step 3: Pushing image to Container Registry..." -ForegroundColor Yellow
docker push "gcr.io/$ProjectId/homechef-api:latest"

# Step 3: Create secrets in Google Cloud Secret Manager
Write-Host "Step 4: Setting up secrets..." -ForegroundColor Yellow
Write-Host ""
Write-Host "Configuring secrets in Google Cloud Secret Manager..." -ForegroundColor Cyan

$secrets = @()
if ($NeonDbUrl) { $secrets += @{ Name = "connection-string"; Value = $NeonDbUrl } }
if ($JwtSigningKey) { $secrets += @{ Name = "jwt-signing-key"; Value = $JwtSigningKey } }
if ($AdminSeedEmail) { $secrets += @{ Name = "admin-seed-email"; Value = $AdminSeedEmail } }
if ($AdminSeedPassword) { $secrets += @{ Name = "admin-seed-password"; Value = $AdminSeedPassword } }
if ($CorsAllowedOrigin) { $secrets += @{ Name = "cors-allowed-origins"; Value = $CorsAllowedOrigin } }

foreach ($secret in $secrets) {
    Write-Host "  Creating secret: $($secret.Name)" -ForegroundColor Gray
    $secret.Value | gcloud secrets create $secret.Name --data-file=- 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "    Secret $($secret.Name) already exists, updating..." -ForegroundColor Gray
        $secret.Value | gcloud secrets versions add $secret.Name --data-file=- 2>$null
    }
}

# Step 4: Deploy to Cloud Run
Write-Host "Step 5: Deploying to Cloud Run..." -ForegroundColor Yellow
gcloud run deploy homechef-api `
    --image "gcr.io/$ProjectId/homechef-api:latest" `
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
Write-Host "=== Deployment Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Service URL: $serviceUrl" -ForegroundColor Cyan
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "1. Update your frontend to use the new API URL: $serviceUrl"
Write-Host "2. Test the API endpoint: $serviceUrl/health"
Write-Host "3. Update CORS secrets if needed"
Write-Host "4. Monitor application logs"
