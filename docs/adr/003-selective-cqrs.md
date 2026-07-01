# ADR-003: CQRS-Lite with Selective Command Layer

**Status:** Accepted  
**Date:** 2026-07-01  
**Deciders:** João Magalhães

## Context

Logic is organised in feature folders under `Features/` with separate command and query interfaces. The risk is ceremony everywhere — pass-through command objects that add indirection without logic.

## Decision

- **Queries** for all read paths that assemble view models (`IFlightReportQuery`, `IOperationsDashboardQuery`, `IAircraftDetailsQuery`, etc.).
- **Commands** only where writes **orchestrate** multiple steps: `IFlightCommands` (lifecycle → validation → recalculation → persist → hangar sync).
- **Airport and Aircraft writes**: controller → repository directly. No `Commands/` folder for these features.

### Rule of thumb

| Write complexity | Pattern |
|------------------|---------|
| CRUD + unique-violation handling | Controller → repository |
| Orchestration (validation, lifecycle, locks, side effects) | Controller → command → repositories |

### Examples

- `AircraftController` injects `IAircraftRepository` and `IAircraftDetailsQuery` — no command layer.
- `FlightCommands` injects five collaborators and runs a multi-step pipeline — earns the command layer.

## Options Considered

### Option A: Selective CQRS (chosen)

| Dimension | Assessment |
|-----------|------------|
| Complexity | Low–Med |
| Cost | Zero |
| Scalability | N/A |
| Team familiarity | High |

**Pros:** Commands where complexity lives; no fake abstractions; controllers stay thin on reads.

**Cons:** Inconsistent pattern across features — intentional, not accidental.

### Option B: Commands everywhere

**Pros:** Uniform structure.

**Cons:** `CreateAirportCommand` that only calls `airportRepository.Create` adds indirection with zero logic.

### Option C: No CQRS — services/repos only

**Pros:** Fewer files.

**Cons:** `FlightCommands` orchestration would land in a controller or a god-service.

## Trade-off Analysis

**Know when not to abstract.** Plain CRUD does not need a command layer; flight mutations genuinely orchestrate validation, lifecycle transitions, per-aircraft locking, and hangar sync.

## Consequences

- Easier: new airport/aircraft CRUD stays fast to implement; flight logic has a clear home.
- Harder: new contributors must learn the rule, not a blanket convention.
- Revisit when: aircraft writes gain orchestration (e.g. maintenance windows blocking flights) → extract `IAircraftCommands`.

## Action Items

1. [x] Structure `Features/` as documented in README
2. [ ] Optional: document the rule in a PR template or contributor note

## Related

- [ADR-002: In-Process Booking Lock](002-in-process-booking-lock.md)
