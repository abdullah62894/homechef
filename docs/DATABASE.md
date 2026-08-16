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

Stage 3: `FoodCategories` (standard category classifications, unique `Slug` index,
`DisplayOrder`) and `FoodItems` (`ChefProfileId` FK with cascade delete,
`CategoryId` FK with set null, `decimal(18,2)` price, `IsAvailable` flag,
`CreatedAtUtc` indexes).

Stage 4: `ChefProfiles` extended with `Address` (varchar(250)), `Latitude` (double precision),
and `Longitude` (double precision). Added single and composite indexes: `IX_ChefProfiles_City`,
`IX_ChefProfiles_Area`, `IX_ChefProfiles_City_Area`, `IX_ChefProfiles_Latitude_Longitude`.

Stage 5: `Reviews` (`Id` Guid PK, `ChefProfileId` FK with cascade delete,
`CustomerUserId` FK with cascade delete, `Rating` integer with check constraint `1..5`,
`Comment` varchar(1000), unique index on `(ChefProfileId, CustomerUserId)`,
indexes on `ChefProfileId`, `CustomerUserId`, `CreatedAtUtc`).

Stage 6: `FavoriteChefs` (`Id` Guid PK, `UserId` FK with cascade delete, `ChefProfileId` FK with cascade delete, unique index on `(UserId, ChefProfileId)`, `CreatedAtUtc`) and `FavoriteFoods` (`Id` Guid PK, `UserId` FK with cascade delete, `FoodItemId` FK with cascade delete, unique index on `(UserId, FoodItemId)`, `CreatedAtUtc`).

Later stages add contact requests, photos,
verification, and moderation tables.

## Geographic data

Stage 4 uses Haversine geodesic distance calculation natively with PostgreSQL double-precision coordinate indexing (`Latitude`, `Longitude`) for fast `find near me` / radius filtering without external OS dependencies.