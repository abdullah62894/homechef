# Architecture

HomeChef is a **modular monolith**. All functionality lives in a single
deployable API, organized so that modules can be extracted into separate
services later if the platform outgrows it.

## High-level topology

```
Internet
   |
Cloudflare            (CDN, TLS, caching — added at deployment)
   |
Next.js               (frontend, Server Components by default)
   |
ASP.NET Core API      (REST, JSON)
   |
PostgreSQL            (single database, one schema)
   |
Object storage        (images — introduced in a later stage)
```

## Backend layers

The backend follows Clean Architecture. Dependencies point inward only.

```
HomeChef.Api            (outermost — HTTP, DI composition root)
   |
HomeChef.Infrastructure (EF Core, PostgreSQL, Identity, external integrations)
   |
HomeChef.Application    (use cases, DTOs, validation, service interfaces)
   |
HomeChef.Domain         (entities, enums, domain rules — no dependencies)
```

Rules enforced by convention:

- Controllers contain no business logic; they delegate to application services.
- EF entities are never returned to clients — DTOs only.
- No synchronous database calls (`ToList()` instead of `ToListAsync()`).
- Projections (`Select(...)`) and `AsNoTracking()` used where appropriate.
- No hard-coded secrets; configuration comes from environment/appsettings.

## Frontend

- Next.js App Router with React Server Components by default.
- Client Components only where interactivity requires them (forms, state).
- API access goes through `src/lib/api/` (single fetch wrapper, consistent
  error handling).
- Tailwind CSS v4, TypeScript strict mode, ESLint + Prettier.

## Key decisions

Recorded as ADRs under [docs/ADR](ADR). Notable ones:

| Decision | Rationale |
| -------- | --------- |
| Modular monolith, not microservices | Simplest correct architecture for an MVP; modules can be split later |
| PostgreSQL + EF Core | Relational integrity, rich queries, low operational cost |
| ASP.NET Core Identity + JWT in an httpOnly cookie | Secure auth without client-side token storage (XSS-resistant) |
| PostgreSQL schema `homechef` | Namespaces all tables; future modules can get their own schema |
| Port 5433 for local dev Postgres | Avoids clashing with an existing local Postgres on 5432 |
| No Redis / no search engine yet | Postgres first; add cache/search only when profiling justifies it |

## Database

See [DATABASE.md](DATABASE.md).

## API

See [API.md](API.md).

## Conventions

- UTC timestamps everywhere (`CreatedAtUtc`, `UpdatedAtUtc`).
- UUIDs for public-facing entity identifiers.
- Consistent response envelope (see API.md).
- Every stage ships with tests for its business rules.