# ADR-005: Server-Local Time Only (No Timezone Handling)

**Status:** Superseded  
**Date:** 2026-07-01  
**Deciders:** João Magalhães

## Context

This ADR originally accepted a server-local time model (`DateTime.Now`) as a temporary limitation.

## Decision

Superseded by the implemented UTC + browser-timezone approach:

- Persist flight timestamps in UTC.
- Capture browser timezone offset in `fo_tz_offset` cookie.
- Convert `datetime-local` input to UTC at the MVC boundary.
- Render UTC in HTML and format local time client-side via `Intl.DateTimeFormat`.
- Use `TimeProvider` as the single source of current time in production code.

## Consequences

- Flight lifecycle, overlap checks, simulation, and dashboard logic now compare UTC with UTC.
- Operators see times in their browser local timezone without coupling to server timezone.
- Dashboard "cancelled today" uses a UTC day window derived from browser offset.

## Action Items

1. [x] Implement `IFlightTimeConverter` and `IUserTimeZoneAccessor`
2. [x] Replace production `DateTime.Now` with `TimeProvider`
3. [x] Remove README timezone limitation section

## Related

- [ADR-006: Single-Instance Deployment](006-single-instance-deployment.md)
