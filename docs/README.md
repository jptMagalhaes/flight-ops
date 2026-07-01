# FlightOps documentation

| Document | Audience | Purpose |
|----------|----------|---------|
| [../README.md](../README.md) | Everyone | Project overview, architecture, quick start |
| [requirements.md](requirements.md) | Product / QA / reviewers | Functional requirements, validations, prerequisites |
| [navigation-map.md](navigation-map.md) | Everyone | Site map, user flows, Mermaid diagrams |
| [decisions-map.md](decisions-map.md) | Architects / contributors | Why X not Y — functional + technical decisions |
| [onboarding.md](onboarding.md) | New contributors | Environment setup, codebase map, common tasks |
| [api.md](api.md) | Integrators / API consumers | REST endpoint reference with examples |
| [runbook.md](runbook.md) | Operators | Deploy, health checks, troubleshooting, rollback |
| [adr/README.md](adr/README.md) | Architects / reviewers | Architecture Decision Records |

## Quick links

- **Run locally:** `dotnet run` — see [onboarding.md](onboarding.md#first-run)
- **Run tests:** `dotnet test --filter "Category!=E2E"` — 59 unit/integration tests
- **Health probe:** `GET /health` — see [runbook.md](runbook.md#health-checks)
- **Swagger (Development only):** `/swagger`
