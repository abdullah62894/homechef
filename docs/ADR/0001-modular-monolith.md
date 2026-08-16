# ADR-0001: Modular monolith instead of microservices

## Status

Accepted

## Problem

The platform must be production-quality, scalable, and cheap to run during the
MVP, without committing to premature distributed complexity.

## Decision

Build a modular monolith: a single deployable ASP.NET Core API containing
clearly separated modules (Domain / Application / Infrastructure / API).
Modules communicate through in-process interfaces so any module can later be
extracted into its own service.

## Reason

- One deployable unit keeps MVP deployment cheap and operations simple.
- Clean module boundaries preserve the option to split later without a rewrite.
- Avoids the operational cost of microservices (networking, observability,
  deployment) before traffic justifies it.

## Alternatives

- Microservices from day one — rejected: high cost, no concrete need yet.
- One giant project without layers — rejected: violates maintainability goals.

## Trade-offs

- A future split will require care to keep module boundaries honest.
- Vertical scaling is the ceiling until a split happens.