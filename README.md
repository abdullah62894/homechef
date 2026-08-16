# HomeChef

A platform to **discover home-based food chefs and small independent food providers** — browse their menus, see locations, contact them, and leave ratings and reviews.

Built for high performance, low cost, SEO, mobile-first usage, and future growth — as a **modular monolith** that can evolve without premature microservices.

## Repository layout

```
├── backend/                  ASP.NET Core (C#) — clean architecture monolith
│   ├── HomeChef.slnx
│   └── src/
│       ├── HomeChef.Domain          Entities, enums, domain rules
│       ├── HomeChef.Application     Use cases, DTOs, services, validation
│       ├── HomeChef.Infrastructure  EF Core + PostgreSQL, Identity, migrations
│       └── HomeChef.Api             REST controllers, middleware, configuration
├── frontend/                 Next.js (App Router) + TypeScript + Tailwind CSS
├── infrastructure/           Scripts, Docker, deployment config
│   └── scripts/              setup-dev.ps1, start-api.ps1, start/stop-postgres
└── docs/                     README-style documentation and ADRs
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 24 LTS](https://nodejs.org/) (with npm)
- [PostgreSQL 17/18](https://www.postgresql.org/download/) — or Docker

## Quick start (development)

### 1. Start PostgreSQL

On Windows (no admin needed), run the setup script. It initializes a local cluster
under your user profile (port **5433**), starts it detached, and creates the
`homechef` and `homechef_test` databases:

```powershell
powershell -ExecutionPolicy Bypass -File infrastructure\scripts\setup-dev.ps1
```

> Note: the local dev Postgres runs on port **5433** to avoid clashing with any
> pre-existing Postgres service on the default port 5432.

### 2. Run the backend API

```powershell
powershell -ExecutionPolicy Bypass -File infrastructure\scripts\start-api.ps1
# or directly:
dotnet run --project backend/src/HomeChef.Api
```

The API listens on `http://localhost:5050`.

- Health check: `GET http://localhost:5050/health`
- OpenAPI (dev): `GET http://localhost:5050/openapi/v1.json`

Apply database migrations (the app also auto-migrates in Development):

```powershell
dotnet ef database update --project backend/src/HomeChef.Infrastructure --startup-project backend/src/HomeChef.Api
```

### 3. Run the frontend

```powershell
cd frontend
cp .env.example .env   # adjust NEXT_PUBLIC_API_URL if needed
npm install
npm run dev
```

Open `http://localhost:3000`.

## Tests

```powershell
dotnet test backend/HomeChef.slnx          # backend unit + integration/API tests
npm test --prefix frontend                  # frontend tests
```

## Current stage status

| Stage | Status |
| ----- | ------ |
| Stage 0 — Architecture & repository setup | Done |
| Stage 1 — Users and authentication | Done |
| Stages 2+ | Planned |

See [docs/STAGES.md](docs/STAGES.md) for the full roadmap.

## Documentation

- [ARCHITECTURE.md](docs/ARCHITECTURE.md) — layers, decisions, conventions
- [API.md](docs/API.md) — endpoints and the response contract
- [DATABASE.md](docs/DATABASE.md) — schema and conventions
- [DEPLOYMENT.md](docs/DEPLOYMENT.md) — deployment strategy
- [docs/ADR](docs/ADR) — architecture decision records