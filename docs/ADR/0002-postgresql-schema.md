# ADR-0002: Single PostgreSQL database, dedicated schema

## Status

Accepted

## Problem

Need one database that is cheap to operate, relational, and able to support
location-based and full-text queries as the platform grows.

## Decision

Use PostgreSQL as the single database. All tables live in the `homechef`
schema. PostGIS and PostgreSQL full-text search are introduced when their
stages arrive; no dedicated search engine or cache in the MVP.

## Reason

- Relational integrity and rich SQL (including future PostGIS + FTS).
- Low operational cost; managed PostgreSQL is available everywhere.
- One schema namespaces the whole domain; future modules can take their own
  schemas without schema churn.

## Alternatives

- MySQL — viable but weaker geo/full-text story.
- MongoDB — rejected: strong relationships between chefs, foods, reviews.
- Elasticsearch/Meilisearch now — rejected: premature; Postgres suffices.

## Trade-offs

- Very high search/geo loads would eventually need a dedicated engine.
- All load concentrates on one database until scaling justifies replicas.