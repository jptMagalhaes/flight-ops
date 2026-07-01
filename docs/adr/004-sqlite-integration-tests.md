# ADR-004: Real SQLite In-Memory for Integration Tests (Not EF InMemory)

**Status:** Accepted  
**Date:** 2026-07-01  
**Deciders:** João Magalhães

## Context

Production code uses bulk `ExecuteUpdateAsync` and `ExecuteDeleteAsync` for hangar sync and lifecycle transitions. EF Core's **InMemory provider does not implement these APIs** — tests built on InMemory would pass while equivalent production code fails silently against SQLite or SQL Server.

## Decision

Integration tests use **real SQLite** `:memory:` with an open `SqliteConnection` held for the lifetime of the context. See `FlightOps.Tests/Support/TestDbContextFactory.cs`.

```csharp
_connection = new SqliteConnection("Data Source=:memory:");
_connection.Open();
// Connection must stay open — closing it drops the in-memory database.
```

## Options Considered

### Option A: SQLite in-memory (chosen)

| Dimension | Assessment |
|-----------|------------|
| Complexity | Low |
| Cost | Zero |
| Fidelity | High |
| CI speed | Good |

**Pros:** Same SQL dialect as production; bulk updates work; catches provider-specific bugs.

**Cons:** Slightly slower than InMemory; connection must stay open for the test lifetime.

### Option B: EF InMemory

**Pros:** Fast; no connection management.

**Cons:** **False confidence** on bulk-update paths — disqualifying for this codebase.

### Option C: Testcontainers SQL Server

**Pros:** Production-like if migrating to Azure SQL.

**Cons:** Docker dependency in CI; overkill while the app targets SQLite.

## Trade-off Analysis

Test fidelity beats test speed for the code paths that rely on bulk EF APIs. This is a deliberate guardrail, not an oversight.

## Consequences

- Easier: integration tests reflect production EF behaviour.
- Harder: InMemory shortcuts are unavailable; reload tests need `ChangeTracker.Clear()` to avoid EF tracking conflicts.
- Revisit when: migrating to SQL Server → consider Testcontainers or a shared SQLite file strategy.

## Action Items

1. [x] `TestDbContextFactory` with open connection
2. [x] Lifecycle and `FlightCommands` integration tests
3. [ ] CI/docs note that EF InMemory is intentionally not used

## Related

- [ADR-001: SQLite on Azure App Service](001-sqlite-azure-app-service.md)
