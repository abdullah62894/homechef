#!/bin/bash
# HomeChef API - Google Cloud Run Deployment Script
# This script deploys the API to Google Cloud Run with Neon DB

set -e

# Configuration
PROJECT_ID="${GCP_PROJECT_ID:-your-project-id}"
REGION="${GCP_REGION:-us-central1}"
SERVICE_NAME="homechef-api"
IMAGE_NAME="gcr.io/${PROJECT_ID}/${SERVICE_NAME}"

echo "=== HomeChef API Deployment to Google Cloud Run ==="
echo "Project: ${PROJECT_ID}"
echo "Region: ${REGION}"
echo "Service: ${SERVICE_NAME}"
echo ""

# Step 1: Enable required APIs
echo "Step 1: Enabling required Google Cloud APIs..."
gcloud services enable run.googleapis.com
gcloud services enable containerregistry.googleapis.com
gcloud services enable cloudbuild.googleapis.com

# Step 2: Build and push Docker image
echo "Step 2: Building Docker image..."
docker build -t ${IMAGE_NAME}:latest -f infrastructure/docker/Dockerfile.api .

echo "Step 3: Pushing image to Container Registry..."
docker push ${IMAGE_NAME}:latest

# Step 4: Set up secrets in Google Cloud Secret Manager
echo "Step 4: Setting up secrets..."
echo ""
echo "IMPORTANT: You need to create the following secrets in Google Cloud Secret Manager:"
echo "  1. connection-string: Your Neon DB connection string"
echo "  2. jwt-signing-key: Your JWT signing key (at least 32 characters)"
echo "  3. admin-seed-email: Admin email for seeding"
echo "  4. admin-seed-password: Admin password for seeding"
echo "  5. cors-allowed-origins: Your frontend URL (e.g., https://your-frontend.vercel.app)"
echo ""
echo "Example commands to create secrets:"
echo "  echo 'your-neon-connection-string' | gcloud secrets create connection-string --data-file=-"
echo "  echo 'your-jwt-signing-key' | gcloud secrets create jwt-signing-key --data-file=-"
echo "  echo 'admin@yourdomain.com' | gcloud secrets create admin-seed-email --data-file=-"
echo "  echo 'your-secure-admin-password' | gcloud secrets create admin-seed-password --data-file=-"
echo "  echo 'https://your-frontend.vercel.app' | gcloud secrets create cors-allowed-origins --data-file=-"
echo ""

# Step 5: Deploy to Cloud Run
echo "Step 5: Deploying to Cloud Run..."
gcloud run deploy ${SERVICE_NAME} \
  --image ${IMAGE_NAME}:latest \
  --region ${REGION} \
  --platform managed \
  --allow-unauthenticated \
  --memory 256Mi \
  --cpu 1 \
  --min-instances 0 \
  --max-instances 2 \
  --port 8080 \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production,DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false,DOTNET_USE_POLLING_FILE_WATCHER=true,DOTNET_EnableDiagnostics=0,Database__AutoMigrate=true" \
  --set-secrets "ConnectionStrings__Default=connection-string:latest,Jwt__SigningKey=jwt-signing-key:latest,Admin__SeedAdminEmail=admin-seed-email:latest,Admin__SeedAdminPassword=admin-seed-password:latest,Cors__AllowedOrigins__0=cors-allowed-origins:latest"

echo ""
echo "=== Deployment Complete ==="
echo "Service URL: $(gcloud run services describe ${SERVICE_NAME} --region ${REGION} --format='value(status.url)')"
echo ""
echo "Next steps:"
echo "1. Update your frontend to use the new API URL"
echo "2. Test the API endpoint"
echo "3. Update DNS if using custom domain"
