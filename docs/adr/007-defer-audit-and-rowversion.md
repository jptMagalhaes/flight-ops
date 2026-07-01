# ADR-007: Defer Entity Audit Columns and RowVersion (Future Replacement for Booking Lock)

**Status:** Accepted  
**Date:** 2026-07-01  
**Deciders:** João Magalhães

## Context

Entities (`Flight`, `Aircraft`, `Airport`) have no audit metadata:

- No `CreatedAt` / `UpdatedAt`
- No `CreatedBy` / `ModifiedBy`
- No optimistic concurrency token (`RowVersion`)

Today, **who changed what and when** is only inferable indirectly (Identity session + request logs). That is acceptable for the current demo/portfolio scope but is a known gap.

Concurrent flight booking is handled by **ADR-002**: an in-process `AircraftBookingLock` (`SemaphoreSlim` per `aircraftId`) serializes validate-then-save inside `FlightCommands`. This works on a single instance (ADR-006) but does not scale out and is application-level locking rather than database-enforced correctness.

## Decision

**Defer** audit columns and `RowVersion` until after the current delivery milestone. Document the intended future shape here so the omission is conscious, not accidental.

When implemented, **`RowVersion` on `Aircraft` (minimum) should replace `AircraftBookingLock`**, not supplement it indefinitely.

## Target model (future)

### Audit (`IAuditable`)

Shared interface on mutable entities:

```csharp
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    string? CreatedById { get; set; }
    string? ModifiedById { get; set; }
}
```

Populate in a `SaveChangesInterceptor` (or `SaveChanges` override) using:

- `TimeProvider` for UTC timestamps
- `IHttpContextAccessor` → `UserManager.GetUserId()` for actor IDs
- Set `Created*` on `Added`, `Updated*` + `ModifiedById` on `Modified`

**Scope:** `Flight`, `Aircraft`, `Airport` at minimum. Identity tables already have their own audit semantics.

### Optimistic concurrency (`RowVersion`)

```csharp
[Timestamp]
public byte[] RowVersion { get; set; } = null!;
```

**Primary target:** `Aircraft` — every flight create/update/delete that affects scheduling must load the aircraft row **tracked**, mutate it in the **same `SaveChanges` / transaction** as the flight write (e.g. bump `UpdatedAt`), so SQLite/EF increments `RowVersion`.

On `DbUpdateConcurrencyException`:

1. Reload aircraft + re-run `FlightScheduleValidator`
2. Return a user-facing conflict (or bounded retry)

This serializes conflicting bookings **at the database**, including under multi-instance deployment — removing the need for `AircraftBookingLock`.

**Note:** bulk `ExecuteUpdateAsync` paths (e.g. `HangarLocationSynchronizer`) must either participate in the same concurrency story or remain lifecycle-only writes that do not overlap with interactive booking.

## Options Considered

### Option A: Defer audit + RowVersion (chosen for now)

| Dimension | Assessment |
|-----------|------------|
| Complexity | Zero today |
| Cost | Zero |
| Correctness | Relies on ADR-002 lock + ADR-006 single instance |

**Pros:** Unblocks current delivery; ADR records the plan.

**Cons:** No forensic trail; lock remains single-instance only.

### Option B: Implement audit only (no RowVersion)

**Pros:** Cheap win for traceability.

**Cons:** Does not address booking concurrency or scale-out; lock still required.

### Option C: Implement RowVersion + audit together (future full fix)

**Pros:** Removes semaphore; enables multi-instance; audit trail for mutations.

**Cons:** ~1–2 h focused work: migration, interceptor, refactor `FlightCommands` transaction boundary, concurrency tests, remove `AircraftBookingLock`.

## Trade-off Analysis

For a portfolio demo with two seeded roles and ADR-006 single-instance deployment, **ADR-002 is sufficient today**. Audit + `RowVersion` are the right **next hardening step** when:

- Forensics matter (“who cancelled this flight?”)
- Scale-out is considered (ADR-006 revisit)
- Parallel booking integration tests are added

Implementing audit without `RowVersion` would add columns but leave the architectural debt of the in-process lock.

## Consequences

- **Easier now:** ship without migration churn or refactoring `FlightCommands` under time pressure.
- **Harder later:** one migration touching all main entities; booking flow must load/update `Aircraft` in the same unit of work; tests for `DbUpdateConcurrencyException` path.
- **When implemented:** mark ADR-002 as **Superseded**; update ADR-006 scale-out notes; delete `Infrastructure/AircraftBookingLock.cs`.

## Action Items

1. [ ] Add `IAuditable` + `SaveChangesInterceptor`
2. [ ] Add `RowVersion` to `Aircraft` (evaluate `Flight` for edit conflicts)
3. [ ] Refactor `FlightCommands` create/update/delete: tracked aircraft load + same-transaction touch
4. [ ] Handle `DbUpdateConcurrencyException` → validation retry or conflict response
5. [ ] Integration test: parallel `Task.WhenAll` bookings on same aircraft (one succeeds, one conflicts)
6. [ ] Remove `AircraftBookingLock` and supersede ADR-002
7. [ ] Migration + update integration tests

## Related

- [ADR-002: In-Process Booking Lock](002-in-process-booking-lock.md) — current mitigation; to be superseded
- [ADR-006: Single-Instance Deployment](006-single-instance-deployment.md) — lock assumption relaxes once RowVersion is in place
