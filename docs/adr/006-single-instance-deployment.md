# ADR-006: Single-Instance Deployment Assumption

**Status:** Accepted  
**Date:** 2026-07-01  
**Deciders:** João Magalhães

## Context

Several components assume **one running process**. Scaling Azure App Service to N instances breaks specific behaviours without prior migration work.

## Decision

Target deployment: **one App Service instance**. Do not scale out without addressing the blockers listed below.

## What Breaks on Horizontal Scale

| Component | Single-instance assumption | Multi-instance failure |
|-----------|---------------------------|------------------------|
| `AircraftBookingLock` | In-process semaphores | Each instance has its own lock → TOCTOU race returns |
| SQLite file on `/home` | Local file per instance | **Split brain** — each instance has a different database |
| `FlightLifecycleBackgroundService` | One timer per process | N redundant timers; races if SQLite were somehow shared |
| Simulation HTTP polling | Stateless | OK if database is shared |
| Cookie auth | Stateless | OK with shared data store |

**Critical blockers:** ADR-001 (SQLite per instance) + ADR-002 (in-process lock).

## Options Considered

### Option A: Stay single-instance (chosen)

| Dimension | Assessment |
|-----------|------------|
| Complexity | Low |
| Cost | Minimal |
| Availability | Single point of failure |

**Pros:** Coherent with all current ADRs; simplest operations.

**Cons:** No HA; App Service restart causes brief downtime.

### Option B: Scale out with shared SQL Server + distributed lock

**Pros:** Real HA path.

**Cons:** Requires superseding ADR-001, adding Redis or SQL-based locking, and increased cost.

### Option C: Scale out with Azure Files mounted SQLite

**Pros:** Cheaper than Azure SQL.

**Cons:** SQLite on network filesystems is officially discouraged; corruption risk under concurrent writers.

## Trade-off Analysis

For a portfolio/demo app, single instance is a **coherent architecture**, not an accident — multiple ADRs reinforce it. Scale-out is a deliberate migration project, not a configuration toggle.

## Consequences

- Easier: no distributed systems problems in v1.
- Harder: traffic spikes cannot be handled by adding instances.
- Revisit when: an uptime SLA or load requirement demands HA → migrate the database first, then replace the booking lock.

## Migration order (if scale is required)

1. Migrate from SQLite to Azure SQL (or Postgres) — supersedes ADR-001.
2. Replace `AircraftBookingLock` with a distributed lock or database-level overlap constraint — supersedes ADR-002.
3. Increase App Service instance count.
4. Revisit ADR-005 if multi-region timezone correctness becomes a requirement.

## Action Items

1. [x] Document SQLite single-writer constraint in README
2. [ ] Azure deploy docs: keep instance count = 1 explicitly
3. [ ] If scaling: follow migration order above

## Related

- [ADR-001: SQLite on Azure App Service](001-sqlite-azure-app-service.md)
- [ADR-002: In-Process Booking Lock](002-in-process-booking-lock.md)
- [ADR-005: Server-Local Time](005-server-local-time.md)
