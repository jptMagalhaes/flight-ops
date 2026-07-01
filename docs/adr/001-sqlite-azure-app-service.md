# ADR-001: SQLite on Azure App Service Instead of Azure SQL

**Status:** Accepted  
**Date:** 2026-07-01  
**Deciders:** João Magalhães

## Context

FlightOps is a portfolio/demo app deployed to Azure App Service (Linux, .NET 9). It needs persistent storage for Identity and flight data, CI/CD, and zero extra infrastructure cost. Traffic is low; there is no multi-region or HA requirement.

## Decision

Use **SQLite** in production with the database file at `/home/flightops.db` on App Service's built-in persistent storage. **Do not** provision Azure SQL.

Connection string via App Service configuration: `Data Source=/home/flightops.db`.

## Options Considered

### Option A: SQLite on App Service `/home`

| Dimension | Assessment |
|-----------|------------|
| Complexity | Low |
| Cost | ~€0 extra |
| Scalability | Single-instance only |
| Team familiarity | High (.NET + EF Core) |

**Pros:** Zero-config locally and in production; same provider in dev/test/prod; F1 tier viable; Docker mount pattern already works (`./data` → `/app/data`).

**Cons:** No horizontal scale; SQLite write concurrency limits; no managed backups/PITR; file corruption risk if storage is misconfigured.

### Option B: Azure SQL

| Dimension | Assessment |
|-----------|------------|
| Complexity | Med |
| Cost | ~€5–15+/mo minimum |
| Scalability | High |
| Team familiarity | High |

**Pros:** Multi-instance safe; proper backups; better concurrent writes.

**Cons:** Over-provisioned for demo traffic; extra secrets/connection management; contradicts the cost-conscious portfolio goal.

## Trade-off Analysis

Correctness for the **actual** deployment model (one App Service instance, demo load) beats theoretical scale. SQLite + EF Core 9 is sufficient; Azure SQL would solve problems this app does not have yet.

## Consequences

- Easier: local dev equals production parity; no cloud database to manage; README stays honest about scope.
- Harder: horizontal scale is blocked until a database migration; concurrent write bursts (many Operators booking simultaneously) hit SQLite limits first.
- Revisit when: traffic exceeds single-instance capacity, HA is required, or more than one App Service instance is needed.

## Action Items

1. [x] Document in README (Azure deploy section)
2. [ ] Add health check monitoring on `/health` in Azure portal
3. [ ] If scale is needed → supersede this ADR with a SQL Server/Postgres migration plan

## Related

- [ADR-006: Single-Instance Deployment](006-single-instance-deployment.md)
