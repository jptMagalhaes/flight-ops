# FlightOps — Contributor onboarding

Get from clone to a running app in under five minutes, then learn where things live.

---

## Prerequisites

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/9) | 9.x |
| Git | any recent |
| (Optional) Docker Desktop | for container workflow |
| (Optional) Playwright browsers | for E2E tests only |

---

## First run

```bash
git clone https://github.com/youputmoeda/FlightOps.git
cd FlightOps
dotnet run
```

On first startup the app will:

1. Apply EF Core migrations (SQLite + Identity tables)
2. Seed 30+ airports, 41 aircraft, 8 sample flights
3. Create roles (`Operator`, `Viewer`) and demo accounts

Open the URL printed in the console (typically `https://localhost:7xxx`), log in, and land on the operations dashboard.

**Demo credentials:**

| Role | Email | Password |
|------|-------|----------|
| Operator (full access) | `operator@flightops.demo` | `Operator123!` |
| Viewer (read-only) | `viewer@flightops.demo` | `Viewer123!` |

The SQLite file (`flightops.db`) is gitignored — every fresh clone starts clean.

### Docker alternative

```bash
docker compose up --build
# → http://localhost:8080
```

Database persists in `./data/flightops.db` via volume mount.

---

## Solution layout

```
FlightOps.sln
├── FlightOps/              # ASP.NET Core MVC web app
├── FlightOps.Tests/        # xUnit unit + integration (59 tests)
└── FlightOps.E2E/          # Playwright browser tests (8 scenarios)
```

---

## Where to find things

### By feature (start here for business logic)

| Area | Path | Notes |
|------|------|-------|
| Flights (core domain) | `Features/Flights/` | Commands, queries, scheduling, simulation, calculator |
| Aircraft | `Features/Aircrafts/` | Details query, hangar sync |
| Airports | `Features/Airports/` | Details query only |
| Dashboard | `Features/Home/` | Operations KPI aggregation |

**Rule of thumb:** if it orchestrates validation + multiple repos → look in `Commands/`. Plain CRUD → controller talks to repository directly (Airports, Aircraft).

### By layer

| Layer | Path |
|-------|------|
| HTTP entry | `Controllers/` (+ `Controllers/Api/` for REST) |
| Persistence | `Repositories/`, `Data/FlightOpsDbContext.cs` |
| Entities | `Entities/`, `Enums/` |
| View models | `Models/Forms/`, `Models/Pages/`, `Models/Components/` |
| Razor views | `Views/` |
| Cross-cutting | `Infrastructure/` (locks, background service, middleware) |
| i18n | `Resources/SharedResources*.resx` |
| Architecture decisions | `docs/adr/` |

### Key flows to trace once

1. **Create flight:** `FlightController.Create` → `IFlightCommands.CreateFlightAsync` → validator → calculator → repository → hangar sync
2. **Lifecycle transitions:** `FlightLifecycleBackgroundService` (every 60s) + `FlightLifecycleApplier` on mutating requests
3. **Simulation poll:** `SimulationController` → `IFlightSimulator.GetActiveFlightsAsync` → Cesium in `wwwroot/js/simulation.js`
4. **Auth:** `AccountController` → Identity cookie → `[Authorize(Policy = "...")]` on controllers

---

## Common tasks

### Run unit/integration tests

```bash
dotnet test --filter "Category!=E2E"
```

Uses real SQLite in-memory (not EF `InMemory`) — see [ADR-004](adr/004-sqlite-integration-tests.md).

### Run E2E tests

Requires a running app instance:

```bash
dotnet build
pwsh FlightOps.E2E/bin/Debug/net9.0/playwright.ps1 install chromium
dotnet run --urls http://localhost:5198   # terminal 1
PLAYWRIGHT_BASE_URL=http://localhost:5198 dotnet test FlightOps.E2E   # terminal 2
```

E2E creates a unique aircraft per run to avoid schedule conflicts with seeded data.

### Add a migration

```bash
dotnet ef migrations add <Name> --project FlightOps
dotnet ef database update --project FlightOps
```

### Add a localized string

1. Add key to `Resources/SharedResources.resx`
2. Mirror in `SharedResources.pt-PT.resx` and `SharedResources.de-DE.resx`
3. Reference via `@Localizer["Key"]` or `[Display(Name = "Key")]`

Or run `python scripts/gen_resx.py` if syncing from a source file.

### Change connection string

`appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=flightops.db"
}
```

Override with env var `ConnectionStrings__DefaultConnection`.

---

## Conventions

- **Namespaces mirror folders** — `FlightOps.Features.Flights.Domain`, etc.
- **Controllers stay thin** — no business logic; delegate to commands/queries.
- **UTC everywhere server-side** — `TimeProvider`, browser offset via `fo_tz_offset` cookie for display.
- **Secure by default** — fallback auth policy requires login unless `[AllowAnonymous]`.
- **File size** — keep files under ~400 lines; split when a class grows.

---

## Further reading

| Doc | When |
|-----|------|
| [../README.md](../README.md) | Architecture deep dive, tech stack |
| [requirements.md](requirements.md) | What the app must do — validations, prerequisites |
| [navigation-map.md](navigation-map.md) | Routes, flows, diagrams |
| [decisions-map.md](decisions-map.md) | Why features and architecture choices exist |
| [api.md](api.md) | REST endpoint reference |
| [runbook.md](runbook.md) | Production ops, Azure deploy |
| [adr/README.md](adr/README.md) | Why SQLite, in-process lock, selective CQRS |

---

## Getting unstuck

| Symptom | Check |
|---------|-------|
| Port already in use | `netstat -ano \| findstr :5198` then `taskkill /PID <pid> /F` |
| Migration errors on startup | Delete local `flightops.db` and restart (dev only) |
| E2E login fails | App must be running at `PLAYWRIGHT_BASE_URL` |
| Integration test passes but prod fails | Likely using EF `InMemory` — this project uses SQLite in-memory instead |
| Localization key shows raw name | RESX manifest must match `FlightOps.Resources.SharedResources` — do not set `ResourcesPath` in `Program.cs` |
