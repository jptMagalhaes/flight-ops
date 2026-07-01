# Architecture Decision Records

Records of significant architectural decisions for FlightOps.

| ADR | Title | Status |
|-----|-------|--------|
| [001](001-sqlite-azure-app-service.md) | SQLite on Azure App Service Instead of Azure SQL | Accepted |
| [002](002-in-process-booking-lock.md) | In-Process SemaphoreSlim for Aircraft Booking Concurrency | Accepted |
| [003](003-selective-cqrs.md) | CQRS-Lite with Selective Command Layer | Accepted |
| [004](004-sqlite-integration-tests.md) | Real SQLite In-Memory for Integration Tests | Accepted |
| [005](005-server-local-time.md) | Server-Local Time Only (No Timezone Handling) | Superseded |
| [006](006-single-instance-deployment.md) | Single-Instance Deployment Assumption | Accepted |
| [007](007-defer-audit-and-rowversion.md) | Defer Entity Audit Columns and RowVersion | Accepted |

## Dependency overview

```
ADR-001 (SQLite prod) ──┐
ADR-002 (In-process lock) ──┼──► ADR-006 (Single instance)
ADR-005 (No timezone) ──┘

ADR-004 (Test SQLite) ──► ADR-001

ADR-003 (Selective CQRS) ──► ADR-002

ADR-007 (Audit + RowVersion) ──► supersedes ADR-002 when implemented
```

## Format

New ADRs follow the template in each file: Context → Decision → Options → Trade-offs → Consequences → Action Items. Number sequentially (`007-...`).
