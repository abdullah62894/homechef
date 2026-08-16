# ADR-0004: Local development PostgreSQL without admin rights

## Status

Accepted

## Problem

The development machine has no admin rights (no service creation, no Docker,
no scheduled tasks) and a stray PostgreSQL instance already occupies port 5432.

## Decision

Run a local PostgreSQL cluster owned by the current user:

- Data directory: `%LOCALAPPDATA%\PostgreSQL\18\data`
- Port: **5433**
- Superuser: `postgres` (dev password only; never used in production)
- Start/stop via `pg_ctl` launched detached with `Start-Process`
  (`infrastructure/scripts/start-postgres.cmd`, `setup-dev.ps1`)

Databases: `homechef` (dev) and `homechef_test` (tests).

## Reason

- No admin rights are required to `initdb`/run a user-owned cluster.
- Detached `Start-Process` avoids the process-tree cleanup that kills
  long-running children of the automation shell.
- Port 5433 sidesteps the existing 5432 instance.

## Alternatives

- Install Docker Desktop — needs admin and is heavier.
- Register a Windows service — needs admin.
- Use the stray 5432 instance — its credentials are unknown.

## Trade-offs

- Postgres starts on demand rather than as a boot service; the setup script
  makes this one command.
- Dev password is committed in `appsettings.Development.json` (local only;
  production config is env-driven and git-ignored).