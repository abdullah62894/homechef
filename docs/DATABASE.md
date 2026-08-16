# Database

- Provider: **PostgreSQL** (via EF Core + Npgsql).
- All tables live in the `homechef` schema (including the `__EFMigrationsHistory` table).
- Local dev database: `homechef` on `127.0.0.1:5433`.
- Test database: `homechef_test` on `127.0.0.1:5433`.

## Conventions

- **Timestamps**: UTC. Every entity carries `CreatedAtUtc` and, where mutable,
  `UpdatedAtUtc` (`timestamptz` columns).
- **Identifiers**: public-facing entities use `Guid` (UUID) keys.
- **Indexes**: created only where expected queries justify them. Each index is
  added alongside the query it serves.
- **Constraints**: primary keys, foreign keys, unique constraints, and check
  constraints are enforced in the database, not just in the application.
- **Nullability**: nullable reference types mirror nullability in the schema.

## Migrations

Migrations are generated from `HomeChef.Infrastructure` and applied to
`homechef`/`homechef_test` via:

```powershell
dotnet ef database update --project backend/src/HomeChef.Infrastructure --startup-project backend/src/HomeChef.Api
```

To add a migration:

```powershell
dotnet ef migrations add <Name> --project backend/src/HomeChef.Infrastructure --startup-project backend/src/HomeChef.Api
```

The design-time factory (`HomeChefDbContextFactory`) reads
`ConnectionStrings__Default` (env) or falls back to the local dev database, so
migrations can be generated without running the app.

## Current schema

Stage 0: only the `__EFMigrationsHistory` table exists in the `homechef` schema.

Stage 1: ASP.NET Core Identity tables (`AspNetUsers`, `AspNetRoles`, ...) with
`Guid` keys, plus `FirstName`/`LastName` on users.

Stage 2: `ChefProfiles` (one-to-one with `AspNetUsers`, unique `UserId` FK,
`text[]` cuisines). Public discovery lists/detail are read from this table.

Later stages add food items, categories, locations, reviews, favorites,
contact requests, photos, verification, and moderation tables.

## Geographic data

PostGIS will be introduced in Stage 4 (search and locations) for `find near me`
queries. Until then the schema stays plain PostgreSQL.