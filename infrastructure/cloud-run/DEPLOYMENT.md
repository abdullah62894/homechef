# HomeChef API - Google Cloud Run Deployment Guide

This guide walks you through deploying the HomeChef API to Google Cloud Run with Neon PostgreSQL.

## Prerequisites

1. **Google Cloud Account** with billing enabled (free tier available)
2. **Neon DB Account** (free tier available)
3. **Google Cloud SDK (`gcloud`)** installed locally
4. Optional: **Docker** installed locally (not required if deploying via Cloud Build)

## Step 1: Set Up Neon DB

1. Go to [https://neon.tech](https://neon.tech)
2. Sign up for a free account
3. Create a new project
4. Note down your connection string (ADO.NET or URI format)

## Step 2: Set Up Google Cloud

### 2.1 Install Google Cloud SDK
```bash
# Windows (using winget)
winget install Google.CloudSDK

# Or download from: https://cloud.google.com/sdk/docs/install
```

### 2.2 Initialize Google Cloud
```bash
gcloud init
gcloud auth login
gcloud config set project YOUR_PROJECT_ID
```

### 2.3 Enable Required APIs
```bash
gcloud services enable run.googleapis.com
gcloud services enable containerregistry.googleapis.com
gcloud services enable cloudbuild.googleapis.com
gcloud services enable secretmanager.googleapis.com
```

## Step 3: Create Secrets in Google Cloud Secret Manager

```bash
# Create secrets for sensitive configuration
echo 'your-neon-connection-string' | gcloud secrets create connection-string --data-file=-
echo 'your-jwt-signing-key-min-32-chars' | gcloud secrets create jwt-signing-key --data-file=-
echo 'admin@yourdomain.com' | gcloud secrets create admin-seed-email --data-file=-
echo 'your-secure-admin-password' | gcloud secrets create admin-seed-password --data-file=-
echo 'https://your-frontend.vercel.app' | gcloud secrets create cors-allowed-origins --data-file=-
```

## Step 4: Deploy to Google Cloud Run

### Option A: Using Deployment Script (Recommended)
```bash
chmod +x infrastructure/cloud-run/deploy.sh
export GCP_PROJECT_ID="your-project-id"
./infrastructure/cloud-run/deploy.sh
```

### Option B: Using Google Cloud Build
```bash
gcloud builds submit --config infrastructure/cloud-run/cloudbuild.yaml .
```

### Option C: Manual Deployment
```bash
# Build Docker image
docker build -t gcr.io/YOUR_PROJECT_ID/homechef-api:latest -f infrastructure/docker/Dockerfile.api .

# Push to Container Registry
docker push gcr.io/YOUR_PROJECT_ID/homechef-api:latest

# Deploy to Cloud Run
gcloud run deploy homechef-api \
  --image gcr.io/YOUR_PROJECT_ID/homechef-api:latest \
  --region us-central1 \
  --platform managed \
  --allow-unauthenticated \
  --memory 256Mi \
  --cpu 1 \
  --min-instances 0 \
  --max-instances 2 \
  --port 8080 \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production,DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false,DOTNET_USE_POLLING_FILE_WATCHER=true,DOTNET_EnableDiagnostics=0,Database__AutoMigrate=true" \
  --set-secrets "ConnectionStrings__Default=connection-string:latest,Jwt__SigningKey=jwt-signing-key:latest,Admin__SeedAdminEmail=admin-seed-email:latest,Admin__SeedAdminPassword=admin-seed-password:latest,Cors__AllowedOrigins__0=cors-allowed-origins:latest"
```

## Step 5: Update Frontend Configuration

Update your frontend environment variables to point to the new Cloud Run API:

```env
NEXT_PUBLIC_API_URL=https://your-service-name-uc.a.run.app
```

## Step 6: Verify Deployment

1. Check the Cloud Run service URL
2. Test the health endpoint: `https://your-service-name-uc.a.run.app/health`
3. Verify database connectivity
4. Test API endpoints

## Free Tier Limits

### Google Cloud Run Free Tier
- 2 million requests per month
- 360,000 GB-seconds of memory
- 180,000 vCPU-seconds of CPU
- Always-free: 1 instance with 1Gi memory, 1 vCPU

### Neon DB Free Tier
- 0.5 GB storage
- 24/7 compute hours (for the smallest instance)
- 100 hours of compute per month

## Cost Optimization Tips

1. **Set minimum instances to 0** - Scales to zero when not in use
2. **Use smallest memory/CPU** - 256Mi memory, 1 vCPU is sufficient for most APIs
3. **Monitor usage** - Use Google Cloud Console to track free tier usage
4. **Set up billing alerts** - Get notified before exceeding free tier

## Troubleshooting

### Common Issues

1. **Application won't start**
   - Check Cloud Run logs: `gcloud logs read --service=homechef-api --limit=50`
   - Verify all secrets are created correctly

2. **Database connection fails**
   - Verify Neon DB connection string format
   - Check if Neon DB endpoint is accessible
   - Ensure SSL mode is enabled

3. **CORS errors**
   - Update CORS allowed origins secret
   - Ensure frontend URL is correct

## Monitoring

- **Google Cloud Console**: https://console.cloud.google.com/run
- **Neon Dashboard**: https://console.neon.tech
- **Application Logs**: `gcloud logs read --service=homechef-api --follow`

## Next Steps

1. Set up Cloud Monitoring for API metrics
2. Configure Cloud Logging for better log management
3. Set up Cloud CDN for static assets
4. Consider Cloud Load Balancing for production traffic
