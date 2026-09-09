# Performance measurements (Stage 12)

Methodology: `infrastructure/loadtest/loadtest.mjs` against the API running
locally (Kestrel, Debug build, local PostgreSQL 18, small development
dataset), 16 concurrent clients, 10 seconds per run. "Cold" = first sustained
run after boot (cache may be primed by the probe request); "warm" = cache
fully populated. Run with:

```bash
node infrastructure/loadtest/loadtest.mjs http://localhost:5050 /api/foods/categories 16 10
```

## Results (local, small dataset)

| Endpoint | Mode | RPS | P50 (ms) | P95 (ms) | P99 (ms) |
| -------- | ---- | --- | -------- | -------- | -------- |
| `GET /api/foods/categories` | cold | 946 | 16.6 | 20.9 | 26.5 |
| `GET /api/foods/categories` | warm | **1052** | 14.9 | 18.4 | 25.2 |
| `GET /api/foods` (home page) | cold | 791 | 19.3 | 26.9 | 31.2 |
| `GET /api/foods` (home page) | warm | **831** | 18.6 | 23.6 | 30.7 |
| `GET /api/locations` | cold | 558 | 27.3 | 40.0 | 59.8 |
| `GET /api/locations` | warm | **583** | 26.2 | 35.8 | 60.1 |
| `GET /api/chefs` (home page) | warm | 508–545 | 25–29 | 52–54 | 73–100 |
| `GET /api/chefs?page=2` (uncached) | — | 134–151 | 87–100 | 172–245 | 221–1201 |

Zero failed requests across all runs. API process working set ≈ 132 MB
after the runs; CPU saturated at the higher RPS levels (the load generator
and the API shared the same dev machine, so absolute numbers are
conservative).

## Findings

1. **Caching pays off on the hottest paths** — categories +10% RPS and ~12%
   lower P95 warm; foods home page +5% RPS. With the current small dataset
   the database is already fast, so gains are modest locally; they grow with
   dataset size and network distance (e.g. remote cloud database).
2. **The real bottleneck is chef list pagination** — `?page=2` runs at
   ~140 RPS with P99 spikes to 1.2 s. The Stage 4 design loads all matching
   chef profiles into memory for distance/cuisine filtering before
   paginating. With hundreds of chefs this stays acceptable; thousands would
   need SQL-side ordering and keyset pagination (deferred until data volume
   justifies it — "cache only proven bottlenecks").
3. **New composite indexes** (`ChefMessages (ChefProfileId, ReadAtUtc)`,
   `FoodItems (ChefProfileId, IsAvailable)`) cover the unread-count and
   chef-menu queries.

## Caching design

- `IMemoryCache` (free, in-process) caches: food categories (10 min),
  location directory (5 min), unfiltered home pages for chefs/foods (60 s),
  per-chef rating summaries (5 min, invalidated on review create/update/
  delete).
- Response caching middleware adds `Cache-Control: public` headers on
  `GET /api/foods/categories` (10 min) and `GET /api/locations` (5 min).
- Swap for Redis later by replacing `AddMemoryCache()` with
  `AddStackExchangeRedisCache` in `HomeChef.Application.DependencyInjection`.
