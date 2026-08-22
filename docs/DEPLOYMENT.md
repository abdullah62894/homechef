# Deployment

## Target architecture

Low-cost, horizontally scalable, and not tied to a single cloud provider:

```
Internet
   |
Cloudflare            CDN + TLS + WAF
   |
Next.js               (Vercel or Cloudflare)
   |
ASP.NET Core API      (container on Fly.io / Render / VPS / AKS later)
   |
PostgreSQL            (managed)
   |
Object storage        (Cloudflare R2 / AWS S3 / Azure Blob)  — later stage
```

## Image storage caveat (Stage 8)

Uploaded images are currently written to the container's local filesystem
(`Images__StoragePath`, default `uploads/`) and served by the API under
`/uploads/...`. The Render container filesystem is **ephemeral** — images
uploaded on a free-tier deployment are lost on every restart/redeploy. This is
acceptable for development; before production, implement an
`IImageStorage` provider for object storage (Cloudflare R2 / S3 / Azure Blob).
Only the URL columns in PostgreSQL (`PhotoUrl`, `PhotoThumbnailUrl`,
`ImageUrl`, `ImageThumbnailUrl`) need to keep working — they already store
whatever URL the provider returns.

## MVP deployment (Render + Vercel)

- **Frontend**: Vercel (Next.js App Router).
- **Backend**: Render Web Service (Docker container running ASP.NET Core API).
- **Database**: Render PostgreSQL (managed).

---

## Deploying Backend to Render

### Option A: Using Render Blueprint (`render.yaml`)
1. In Render Dashboard, click **New +** -> **Blueprint**.
2. Connect your GitHub repository (`homechef`).
3. Render reads `render.yaml` and provisions:
   - `homechef-db` (PostgreSQL Database)
   - `homechef-api` (Docker Web Service using `infrastructure/docker/Dockerfile.api`)
4. Fill in the missing environment variable secrets in the Render Dashboard:
   - `Jwt__SigningKey`: A random string at least 32 characters long.
   - `Cors__AllowedOrigins__0`: Your Vercel frontend URL (e.g. `https://your-homechef-app.vercel.app`).

### Option B: Manual Web Service Setup on Render
1. Create a **PostgreSQL** database on Render named `homechef-db`.
2. Create a **Web Service** on Render:
   - **Environment**: Docker
   - **Dockerfile Path**: `./infrastructure/docker/Dockerfile.api`
   - **Docker Context**: `.` (root of the repo)
   - **Health Check Path**: `/health`
3. Configure Environment Variables:
   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE`: `false`
   - `DOTNET_USE_POLLING_FILE_WATCHER`: `true`
   - `DOTNET_EnableDiagnostics`: `0`
   - `Database__AutoMigrate`: `true`
   - `ConnectionStrings__Default`: Internal Database URL from `homechef-db`
   - `Jwt__Issuer`: `HomeChef`
   - `Jwt__Audience`: `HomeChefWeb`
   - `Jwt__SigningKey`: (>= 32 chars secret)
   - `Cors__AllowedOrigins__0`: `https://<your-vercel-app>.vercel.app`

---

## Deploying Frontend to Vercel

1. In Vercel Dashboard, click **Add New...** -> **Project**.
2. Import your GitHub repository (`homechef`).
3. Configure Project Settings:
   - **Framework Preset**: Next.js
   - **Root Directory**: `frontend` (Click *Edit* and select `frontend`)
4. Configure Environment Variables:
   - `NEXT_PUBLIC_API_URL`: Your Render backend URL (e.g. `https://homechef-api.onrender.com`)
5. Click **Deploy**.

---

## Troubleshooting Inotify / Exit 134 on Linux Containers
Linux container environments (like Render's shared instances) enforce a tight limit on `inotify` file watchers (often 128 max system-wide). ASP.NET Core defaults to enabling file change watchers for `appsettings.json` on startup.
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