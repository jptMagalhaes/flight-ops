# ADR-002: In-Process SemaphoreSlim for Aircraft Booking Concurrency

**Status:** Accepted  
**Date:** 2026-07-01  
**Deciders:** João Magalhães

## Context

Flight booking is validate-then-save: `FlightScheduleValidator` checks schedule overlap, then `FlightRepository.CreateFlightAsync` persists. Two concurrent requests for the same aircraft can both pass validation before either saves — a classic **TOCTOU** double-booking race.

`FlightCommands` acquires a per-aircraft lock before re-validating and persisting:

```csharp
using IDisposable bookingLock = await AircraftBookingLock.AcquireAsync(flight.AircraftId);

FlightCommandsError? validationError = await _flightScheduleValidator.ValidateAsync(/* ... */);
Flight? created = await _flightRepository.CreateFlightAsync(flight);
```

Implementation: `Infrastructure/AircraftBookingLock.cs` — static `ConcurrentDictionary<int, SemaphoreSlim>`.

## Decision

Serialize create/update/delete per `aircraftId` with an **in-process** `SemaphoreSlim` lock.

## Options Considered

### Option A: In-process SemaphoreSlim (chosen)

| Dimension | Assessment |
|-----------|------------|
| Complexity | Low |
| Cost | Zero |
| Scalability | Single-instance |
| Team familiarity | High |

**Pros:** Fixes the race with ~25 lines; no infrastructure; lock scope is one aircraft, not global.

**Cons:** Ineffective across multiple app instances; static dictionary grows with aircraft IDs (negligible for the 41-aircraft fleet).

### Option B: Database unique constraint + retry

| Dimension | Assessment |
|-----------|------------|
| Complexity | Med |
| Cost | Zero |
| Scalability | Multi-instance |
| Team familiarity | Med |

**Pros:** Works in a distributed deployment; database is source of truth.

**Cons:** Interval overlap is not trivial to express as a SQL constraint; UX degrades to opaque retry on conflict; app-level origin/schedule validation still required.

### Option C: Distributed lock (Redis / SQL advisory)

| Dimension | Assessment |
|-----------|------------|
| Complexity | High |
| Cost | Extra service |
| Scalability | Multi-instance |
| Team familiarity | Low for this project |

**Pros:** Correct under scale-out.

**Cons:** Solves a problem the demo does not have; adds failure modes and operational burden.

## Trade-off Analysis

ADR-001 locks the deployment to a single instance anyway. The in-process lock is the **minimum correct fix** for the actual deployment model. A database-level overlap constraint remains a future hardening step if the app scales.

## Consequences

- Easier: race eliminated on a single node; no external dependencies.
- Harder: scale-out (see ADR-006) requires replacing or supplementing this mechanism.
- Revisit when: more than one App Service instance is deployed, or load tests show lock contention.

## Action Items

1. [x] Implement `AircraftBookingLock`
2. [x] Wrap create/update/delete in `FlightCommands`
3. [ ] Optional: integration test with parallel `Task.WhenAll` on the same `aircraftId`

## Related

- [ADR-003: Selective CQRS](003-selective-cqrs.md)
- [ADR-006: Single-Instance Deployment](006-single-instance-deployment.md)
- [ADR-007: Defer Audit and RowVersion](007-defer-audit-and-rowversion.md) — planned replacement via optimistic concurrency on `Aircraft`
