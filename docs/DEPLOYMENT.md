# Deployment

## Target architecture

Low-cost, horizontally scalable, and not tied to a single cloud provider:

```
Internet
   |
Cloudflare            CDN + TLS + WAF
   |
Next.js               (Vercel)
   |
ASP.NET Core API      (Google Cloud Run container)
   |
PostgreSQL            (Neon serverless / managed)
   |
Object storage        (Cloudflare R2 / AWS S3 / Azure Blob)  — later stage
```

## Image storage caveat (Stage 8)

Uploaded images are currently written to the container's local filesystem
(`Images__StoragePath`, default `uploads/`) and served by the API under
`/uploads/...`. Container filesystems are **ephemeral** — images
uploaded without persistent object storage are lost on container restart/redeploy. This is
acceptable for development; before production, implement an
`IImageStorage` provider for object storage (Cloudflare R2 / S3 / Azure Blob).
Only the URL columns in PostgreSQL (`PhotoUrl`, `PhotoThumbnailUrl`,
`ImageUrl`, `ImageThumbnailUrl`) need to keep working — they already store
whatever URL the provider returns.

## Production deployment (Google Cloud Run + Neon DB + Vercel)

- **Frontend**: Vercel (Next.js App Router).
- **Backend**: Google Cloud Run (container running ASP.NET Core API).
- **Database**: Neon PostgreSQL (serverless managed).

---

## Deploying Backend to Google Cloud Run

### Option A: Direct Source Deploy (Cloud Build)
```bash
gcloud run deploy homechef-api \
  --source . \
  --region us-central1 \
  --platform managed \
  --allow-unauthenticated \
  --memory 256Mi \
  --cpu 1 \
  --min-instances 0 \
  --max-instances 2 \
  --port 8080
```

### Option B: Deployment Script
Use `infrastructure/cloud-run/deploy.sh` or `infrastructure/cloud-run/DEPLOY-NOW.ps1`.

### Secrets Configuration (Secret Manager)
Sensitive configurations are stored in Google Cloud Secret Manager:
- `connection-string`: Neon database connection string
- `jwt-signing-key`: JWT signing secret (>= 32 chars)
- `admin-seed-email`: First admin seed email
- `admin-seed-password`: First admin seed password
- `cors-allowed-origins`: Allowed frontend origin URL

---

## Deploying Frontend to Vercel

1. In Vercel Dashboard, click **Add New...** -> **Project**.
2. Import your repository (`homechef`).
3. Configure Project Settings:
   - **Framework Preset**: Next.js
   - **Root Directory**: `frontend`
4. Configure Environment Variables:
   - `NEXT_PUBLIC_API_URL`: Your Cloud Run backend URL (e.g. `https://homechef-api-xxx.run.app`)
5. Click **Deploy**.

---

## Troubleshooting Inotify / Exit 134 on Linux Containers
Linux container environments enforce a tight limit on `inotify` file watchers (often 128 max system-wide). ASP.NET Core defaults to enabling file change watchers for `appsettings.json` on startup.
This is resolved by:
1. Setting `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false` and `DOTNET_USE_POLLING_FILE_WATCHER=true`.
2. Setting these variables in code in `Program.cs` before `WebApplication.CreateBuilder(args)` is called.

---

## Environment configuration
Secrets are never committed. Configuration precedence:
1. Environment variables (`ConnectionStrings__Default`, `Jwt__...`, etc.)
2. `appsettings.{Environment}.json`
3. `appsettings.json`

`appsettings.Production.json` and any `.env` files are git-ignored.