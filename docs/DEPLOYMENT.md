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

## MVP deployment (Stage 14)

- **Frontend**: Vercel (or Cloudflare Pages). Static + server-rendered pages.
- **Backend**: a single container running the ASP.NET Core API.
- **Database**: managed PostgreSQL (any provider).
- **Images**: Cloudflare R2 (S3-compatible) — introduced with image storage.

No Kubernetes during the MVP. The modular monolith deploys as one unit.

## Local Docker

`infrastructure/docker/` contains skeleton `Dockerfile.api` and
`Dockerfile.web` (refined in Stage 14) and `docker-compose.yml` with a Postgres
service. Native PostgreSQL is the primary local option; the compose file is a
reproducible alternative.

## Environment configuration

Secrets are never committed. Configuration precedence:

1. Environment variables (`ConnectionStrings__Default`, `Jwt__...`, etc.)
2. `appsettings.{Environment}.json`
3. `appsettings.json`

`appsettings.Production.json` and any `.env` files are git-ignored.

## HTTPS, CORS, security headers

Enabled at the ingress (Cloudflare). The API serves HTTPS behind the proxy.
CORS is restricted to configured frontend origins with credentials allowed.