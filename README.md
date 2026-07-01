# FlightOps

![CI](https://github.com/youputmoeda/flight-ops/actions/workflows/ci.yml/badge.svg)

A flight operations management system built with **ASP.NET Core 9** and **Entity Framework Core** — real-time 3D flight simulation on a WebGL globe, role-based access control, automated flight lifecycle management, and a full CRUD back office for a global fleet of aircraft.

> Built as a portfolio project to demonstrate production-shaped engineering: clean architecture, automated testing across three layers, CI/CD, containerization, and cloud deployment — not just a CRUD demo.

---

## Engineering highlights

A few things worth a closer look if you're scanning this before a deeper read:

- **Authentication & RBAC** — ASP.NET Core Identity with two roles (Operator/Viewer), a secure-by-default authorization fallback policy (every endpoint requires login unless explicitly marked otherwise), and UI that hides actions a Viewer isn't allowed to perform — not just server-side enforcement with a leaky client.
- **Concurrency correctness** — found and fixed a genuine TOCTOU race condition in the flight-booking flow (validate-then-save wasn't atomic, so two concurrent requests could double-book the same aircraft) with a per-aircraft async lock — see [`Infrastructure/AircraftBookingLock.cs`](Infrastructure/AircraftBookingLock.cs).
- **Test pyramid, not just unit tests** — 59 unit/integration tests plus 8 Playwright E2E scenarios (auth boundaries, schedule conflicts, REST API, full create→simulate flow). The integration tests run against a real SQLite database, not EF Core's `InMemory` provider, because `InMemory` silently doesn't support the bulk `ExecuteUpdateAsync` calls this app relies on — a gap that would have produced *passing tests for code that doesn't work in production*.
- **CI/CD** — GitHub Actions runs the full test suite on every push/PR; a separate deploy workflow only ships to Azure if the tests pass first.
- **Observability** — a `/health` endpoint backed by an EF Core DB check, structured request logging with correlation IDs, and a background service that keeps flight statuses correct even with zero HTTP traffic.
- **Cost-conscious infra decisions** — the Azure deployment plan deliberately stays on SQLite with the App Service's built-in persistent storage instead of provisioning Azure SQL, because a demo app doesn't need a managed database server, and the README says so explicitly instead of silently over-provisioning.
- **Knows when *not* to add abstraction** — see [Architecture](#architecture) below: two of the five feature areas have no command layer at all, on purpose, because plain CRUD doesn't need one.

---

## Documentation

| Doc | Purpose |
|-----|---------|
| [`docs/requirements.md`](docs/requirements.md) | Functional requirements, validations, prerequisites |
| [`docs/navigation-map.md`](docs/navigation-map.md) | Site map, user flows, Mermaid diagrams |
| [`docs/decisions-map.md`](docs/decisions-map.md) | Functional and technical decision rationale |
| [`docs/onboarding.md`](docs/onboarding.md) | Contributor setup, codebase map, common tasks |
| [`docs/api.md`](docs/api.md) | REST API reference with auth and examples |
| [`docs/runbook.md`](docs/runbook.md) | Deploy, health checks, troubleshooting, rollback |
| [`docs/adr/`](docs/adr/README.md) | Architecture Decision Records |

---

## What it does

FlightOps simulates an airline operations centre. Logged-in users can:

- **Manage airports** — a seeded catalogue of 30+ real-world airports with IATA codes and GPS coordinates
- **Manage aircraft** — a fleet of 41 aircraft across realistic registrations (CS-TUI, A6-EOA, 9V-SMA …), each with cruise speed, fuel consumption rate, and take-off fuel effort
- **Schedule and dispatch flights** — create a flight and the system calculates distance (Haversine), fuel burn, and estimated arrival automatically; a live preview updates as you pick origin/destination/aircraft
- **Watch flights in real time** — the Simulation page renders every in-flight aircraft on a CesiumJS WebGL globe, interpolated along the great-circle arc between airports, with a live fuel burn and progress panel
- **Operations dashboard** — KPI cards (active flights, fleet split ground/airborne, upcoming departures, cancellations today), next departures list, and fleet status at a glance
- **Flight reports** — paginated, filterable history with totals
- **Query a small REST API** — the same data, machine-readable, documented with Swagger

What a user can *do* depends on their role:

| Role | Can view everything | Can create / edit / delete |
|---|---|---|
| **Operator** | ✅ | ✅ |
| **Viewer** | ✅ | ❌ (buttons are hidden, and the server rejects the action even if you bypass the UI) |

---

## Architecture

Architecture decisions are tracked in [`docs/adr`](docs/adr/README.md).

### Vertical slice / feature folders

Logic is organised by **feature**, not by layer. Each domain area owns its commands and queries:

```
Features/
  Airports/
    Queries/    AirportDetailsQuery, IAirportDetailsQuery
  Aircrafts/
    Queries/    AircraftDetailsQuery
    HangarLocationSynchronizer
  Flights/
    Commands/   FlightCommands, IFlightCommands, FlightOperationResult
    Queries/    FlightReportQuery, FlightCalculationPreviewQuery
    Scheduling/ FlightScheduleValidator, AircraftLocationResolver, FlightLifecycleApplier
    Domain/     FlightCalculatorService (Haversine, fuel, duration)
    Simulation/ FlightSimulator, GeoInterpolationService
  Home/
    Queries/    OperationsDashboardQuery
```

Airports and Aircraft have no `Commands/` folder: their writes are plain CRUD with no
orchestration beyond persistence, so controllers call `IAirportRepository`/`IAircraftRepository`
directly instead of through a pass-through command layer that would add indirection without
adding logic. `Flights` keeps its `Commands/` layer because `FlightCommands` genuinely
orchestrates validation, lifecycle transitions, recalculation, and multi-repository writes.

Controllers are thin — they validate input, delegate to a command/query object, and map to a view model. No business logic lives in a controller.

**Folder conventions:** namespaces mirror folder paths (`FlightOps.Models.Forms`, `FlightOps.Features.Flights.Domain`, …). View models split into **Forms** (mutating CRUD), **Pages** (read-only screens), and **Components** (shared partial models). Razor partials under `Views/Shared/` are grouped by concern (`Layout/`, `Lists/`, `Badges/`, `Kpi/`); flight-specific partials live in `Views/Flight/`.

### CQRS-lite

Read side (queries) and write side (commands) are separated behind distinct interfaces. Example:

| Interface                        | Responsibility                                           |
| --------------------------------- | -------------------------------------------------------- |
| `IFlightCommands`                 | Create, update, delete flights with full validation      |
| `IFlightReportQuery`              | Build a paginated, filtered report view model             |
| `IFlightCalculationPreviewQuery`  | Compute distance/fuel/arrival for a live preview call     |
| `IOperationsDashboardQuery`       | Aggregate KPIs for the home dashboard                      |
| `IAircraftDetailsQuery`           | Enrich an aircraft record with its recent flight history   |

### Flight lifecycle state machine

Flights transition through four states with enforced rules:

```
Scheduled ──► Departed ──► Arrived
    │
    └──────────────────────► Cancelled
```

- `Scheduled → Departed` is allowed only if the aircraft is at the correct origin airport at departure time (resolved via `AircraftLocationResolver`)
- `Scheduled → Departed` via form sets `DepartureTime` from the app clock (`TimeProvider`, UTC) automatically
- `Departed` flights cannot have their origin or aircraft changed
- A `Scheduled` flight whose departure window has already passed is automatically cancelled — both on every mutating request *and* on a one-minute background timer, so it stays correct even with zero HTTP traffic (see [Observability & Operations](#observability--operations))

### Booking concurrency

Validating "is this aircraft free?" and saving the new flight are two separate steps. Without
protection, two concurrent booking requests for the same aircraft can both pass validation before
either one saves, double-booking it. `AircraftBookingLock` serializes create/update/delete per
aircraft ID with an in-process `SemaphoreSlim`, which is the right amount of machinery for a
single-instance deployment — a distributed lock would be solving a problem this app doesn't have.

### Aircraft location tracking

`AircraftLocationResolver` answers "where is aircraft X at time T?" by:

1. Finding the most recent **Arrived** flight for that aircraft before `T` and returning `Destination`
2. Falling back to `Aircraft.CurrentAirportId` (its home hangar)

`HangarLocationSynchronizer` keeps `CurrentAirportId` consistent after every mutation — aircraft in the air get `null`, aircraft on the ground get their last destination. This is done with bulk `ExecuteUpdateAsync` for efficiency.

### Schedule conflict detection

`FlightScheduleValidator` prevents double-booking an aircraft:

```csharp
// Classic interval overlap check
departureA < arrivalB && departureB < arrivalA
```

It fetches only the blocking flights (Scheduled or Departed) for that aircraft and runs the check in memory, excluding the flight being edited.

### Geo calculations

Two static services handle all spatial math (under `Features/Flights/`):

- `Domain/FlightCalculatorService` — Haversine formula for great-circle distance, linear fuel burn (`distance × L/km + takeoff_effort`), duration from cruise speed
- `Simulation/GeoInterpolationService` — Slerp (spherical linear interpolation) along the great-circle arc, used by the simulator to place the aircraft icon at the correct lat/lon at any given moment

### Repository pattern

Three repositories (`FlightRepository`, `AircraftRepository`, `AirportRepository`) own all EF Core queries. Repositories use `AsNoTracking()` on reads and `ExecuteUpdateAsync`/`ExecuteDeleteAsync` for bulk writes to avoid change-tracker overhead. They're kept (rather than collapsed into direct `DbContext` use) specifically because `FlightOps.Tests` mocks them — the abstraction earns its cost through actual test coverage, not in theory.

---

## Authentication & Authorization

ASP.NET Core Identity, backed by the same SQLite database, with two roles:

| Policy | Who satisfies it | Used on |
|---|---|---|
| `ViewerOrOperator` | Viewer or Operator | All read views (lists, details, dashboard, simulation) |
| `OperatorOnly` | Operator only | Create, Edit, Delete actions everywhere |
| *(fallback policy)* | Any authenticated user | Everything without an explicit `[Authorize]`/`[AllowAnonymous]` — secure by default, so a new controller can't accidentally ship unauthenticated |

`Login`, `AccessDenied`, and the culture-switcher endpoint are explicitly `[AllowAnonymous]`; everything else requires a session.

**Demo credentials** (seeded automatically on first run):

| Role | Email | Password |
|---|---|---|
| Operator (full access) | `operator@flightops.demo` | `Operator123!` |
| Viewer (read-only) | `viewer@flightops.demo` | `Viewer123!` |

---

## Testing & quality

```bash
dotnet test --filter "Category!=E2E"
```

**`FlightOps.Tests`** — 59 unit/integration tests:

- Unit: `FlightCalculatorService`, `GeoInterpolationService`, `FlightScheduleValidator` (including overlap-interval boundary semantics), `FlightSimulator`, `FlightCalculationPreviewQuery`, `FlightTimeConverter`, `AircraftLocationResolver`
- Integration: `FlightCommands`, `FlightRepository` lifecycle transitions, `HangarLocationSynchronizer`, `AircraftLocationResolver`, dashboard counts — all against a **real SQLite in-memory database**, not EF Core's `InMemory` provider (`InMemory` doesn't implement `ExecuteUpdateAsync`/`ExecuteDeleteAsync`)

**`FlightOps.E2E`** — 8 Playwright scenarios across 4 test classes:

| File | Scenarios |
|------|-----------|
| `AuthorizationTests` | Unauthenticated redirect, Viewer denied on Create, Viewer can browse |
| `FlightApiTests` | Authenticated Viewer calls `/api/flights` and `/api/flights/active` |
| `FlightScheduleConflictTests` | Overlapping booking rejected |
| `FlightCreateAndSimulateTests` | Operator creates unique aircraft → books flight → appears on `/Simulation` |

Excluded from CI via `[Trait("Category", "E2E")]` — needs a running app and Chromium:

```bash
dotnet build
pwsh FlightOps.E2E/bin/Debug/net9.0/playwright.ps1 install chromium
dotnet run --urls http://localhost:5198 &   # in one terminal
PLAYWRIGHT_BASE_URL=http://localhost:5198 dotnet test FlightOps.E2E   # in another
```

See [`docs/onboarding.md`](docs/onboarding.md) for the full contributor workflow.

---

## Observability & operations

- **`GET /health`** — backed by `AddDbContextCheck<FlightOpsDbContext>`, publicly accessible (`[AllowAnonymous]`) so it works as an Azure App Service or load-balancer health probe
- **`FlightLifecycleBackgroundService`** — a `PeriodicTimer`-based hosted service that runs flight status transitions every minute, independent of HTTP traffic; failures are caught and logged per tick rather than crashing the host
- **`RequestLoggingMiddleware`** — logs method, path, status code, and duration for every request, tagged with a correlation ID (`X-Correlation-ID` request header if present, otherwise generated) echoed back in the response

---

## Tech stack

| Layer            | Technology                                                              |
| ----------------- | ------------------------------------------------------------------------ |
| Framework         | ASP.NET Core 9 MVC                                                       |
| Language          | C# 13 (nullable enabled, primary constructors, collection expressions)   |
| ORM               | Entity Framework Core 9                                                  |
| Database          | SQLite (zero-config, file-based)                                         |
| Auth              | ASP.NET Core Identity, cookie-based, role policies                       |
| Frontend          | Bootstrap 5, Bootstrap Icons, Razor Views                                |
| 3D Globe          | CesiumJS 1.119 (WebGL)                                                   |
| API docs          | Swashbuckle / Swagger (Development only)                                 |
| Testing           | xUnit, Moq, Playwright (.NET)                                            |
| CI/CD             | GitHub Actions (test on every push/PR, deploy to Azure on `main`)        |
| Containerization  | Docker (multi-stage build) + Docker Compose                              |
| Localization      | .NET RESX — English, Portuguese (PT), German (DE)                        |
| DI                | Built-in `Microsoft.Extensions.DependencyInjection`                      |

---

## Running locally

**Prerequisites:** [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)

```bash
git clone https://github.com/youputmoeda/flight-ops.git
cd flight-ops
dotnet run
```

On first run the app will apply EF Core migrations (including Identity tables), seed 30+ airports,
41 aircraft, 8 sample flights, the two roles, and the two demo accounts above.

Open `https://localhost:<port>`, log in with either demo account, and the dashboard loads with live data.

> The SQLite file is gitignored, so every fresh clone starts clean.

### Changing the database

The connection string is in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=flightops.db"
}
```

### Running with Docker

```bash
docker compose up --build
```

The `Dockerfile` is a multi-stage build (`sdk:9.0` to publish, `aspnet:9.0` to run) listening on
port 8080. `docker-compose.yml` mounts `./data` to `/app/data` so the SQLite file survives
container restarts, and points the connection string there via
`ConnectionStrings__DefaultConnection`. Open `http://localhost:8080`.

### Deploying to Azure App Service

The deployment plan deliberately stays on SQLite in production — no Azure SQL, no extra storage
resource, no added cost. Azure App Service on Linux already backs `/home` with a persistent file
share included in the App Service plan, so pointing the SQLite file there is enough for it to
survive restarts and redeploys, for the traffic level a demo app actually sees.

1. Create an Azure App Service (Linux, .NET 9 runtime) — the free **F1** tier works for a demo.
2. In the App Service's **Configuration → Application settings**, add:
   - `ConnectionStrings__DefaultConnection` = `Data Source=/home/flightops.db`
   - `ASPNETCORE_ENVIRONMENT` = `Production`
3. (Optional) Configure the App Service's built-in **Health check** to probe `/health`.
4. From the Azure Portal, **Overview → Get publish profile**, then add two GitHub repo secrets:
   - `AZURE_WEBAPP_NAME` — the App Service name
   - `AZURE_WEBAPP_PUBLISH_PROFILE` — the downloaded publish profile XML
5. Push to `main`. [`azure-deploy.yml`](.github/workflows/azure-deploy.yml) runs the test suite
   first and only deploys if it passes.

### REST API

Read-only JSON API at `/api/flights/*`, cookie-authenticated, same `ViewerOrOperator` policy as the UI. Full reference with request/response examples: [`docs/api.md`](docs/api.md).

Swagger UI: `/swagger` (Development only).

---

## Localization

The UI supports three languages, switchable at runtime via a cookie (no page reload required):

| Code    | Language          |
| ------- | ----------------- |
| `en`    | English (default) |
| `pt-PT` | Portuguese        |
| `de-DE` | German            |

All user-facing strings, validation messages, and data annotation errors go through
`Resources/SharedResources.resx` files, resolved via ASP.NET Core's built-in
`IStringLocalizer<SharedResources>`. No custom localizer — the resource manifest name
(`FlightOps.Resources.SharedResources`, derived from the marker class's own namespace) already
matches the embedded `.resx` location, so no `ResourcesPath` configuration is needed either; adding
one would actually double the path and break resolution.

---

## Timezone model

Flight timestamps are stored and processed in UTC end-to-end (`TimeProvider` + UTC comparisons),
while the UI displays times in the operator's browser timezone.

- Create/Edit forms still use `datetime-local`, but browser-local values are converted to UTC at the MVC boundary
- The browser sends its `getTimezoneOffset()` via a cookie (`fo_tz_offset`)
- Shared date/time components render UTC ISO values (`datetime="...Z"`) and format locally with `Intl.DateTimeFormat`
- Dashboard "cancelled today" is computed with a UTC day range derived from the browser offset

---

## Key design decisions worth reading

| File                                                      | Why it's interesting                                                                           |
| ----------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| `Infrastructure/AircraftBookingLock.cs`                    | Fixes a real TOCTOU race condition with the right amount of machinery for a single-instance app    |
| `Features/Flights/Scheduling/FlightScheduleValidator.cs`   | Schedule conflict validation + origin check in one clean class                                      |
| `Features/Flights/Scheduling/AircraftLocationResolver.cs`  | Derives where an aircraft is from its flight history, not stored state                              |
| `Features/Flights/Scheduling/FlightLifecycleApplier.cs`    | Single entry point that applies all time-based status transitions and triggers hangar sync          |
| `Infrastructure/FlightLifecycleBackgroundService.cs`       | Keeps the state machine correct without relying on HTTP traffic to drive it                         |
| `Features/Aircrafts/HangarLocationSynchronizer.cs`         | Bulk EF Core `ExecuteUpdateAsync` to sync fleet locations without loading entities into memory      |
| `Features/Flights/Simulation/FlightSimulator.cs`           | Combines lifecycle, great-circle interpolation, and fuel telemetry into a snapshot model            |
| `Features/Flights/Simulation/GeoInterpolationService.cs`   | Slerp on the unit sphere — pure math, no libraries                                                  |
| `Features/Flights/Domain/FlightCalculatorService.cs`       | Haversine distance, fuel burn, duration — fully deterministic, trivially testable                   |
| `Repositories/Flights/FlightRepository.cs`                 | `ApplyLifecycleTransitionsAsync` — bulk EF updates that drive the entire state machine               |
| `Infrastructure/DbUniqueViolationDetector.cs`               | Detects SQLite unique-constraint violations via `SqliteException.SqliteExtendedErrorCode`, not string-matching the exception message |
| `Features/Flights/Commands/FlightCommands.cs`               | Full create/update orchestration: lifecycle → validation → recalc → persist → sync, behind a per-aircraft lock |
| `FlightOps.E2E/FlightCreateAndSimulateTests.cs`             | Real-browser end-to-end test, written to be safe to run repeatedly against a live, stateful database |

---

## Project structure

```
FlightOps/
├── Controllers/
│   └── Api/               # FlightsApiController (read-only REST API)
├── Data/
│   ├── FlightOpsDbContext.cs   # IdentityDbContext<ApplicationUser>
│   ├── DbSeeder.cs
│   ├── IdentitySeeder.cs       # Roles + demo accounts
│   └── Seed/                   # Static seed data records
├── Entities/                   # EF Core entity classes
├── Enums/                      # FlightStatus, FlightCommandsError
├── Features/                   # Vertical slices (Commands + Queries + domain logic)
├── Helpers/                    # FlightStatusHelper, AirportSelectListHelper
├── Infrastructure/             # AircraftBookingLock, FlightLifecycleBackgroundService,
│                                # RequestLoggingMiddleware, DbUniqueViolationDetector
├── Mappers/                    # Entity ↔ ViewModel extension methods
├── Migrations/                 # EF Core migrations
├── Models/
│   ├── Forms/                  # Create/Edit view models (Flight, Aircraft, Airport, Login)
│   ├── Pages/                  # Read-only page models (details, dashboard, simulation, error)
│   └── Components/             # Reusable partial models (lists, KPI cards, form footer, IATA route)
├── Repositories/                # Data access (EF Core)
├── Resources/                   # SharedResources.resx (+ pt-PT, de-DE variants)
├── Views/
│   ├── Shared/
│   │   ├── Layout/             # _Layout, Error, _ValidationScriptsPartial
│   │   ├── Forms/              # Entity form open/close/footer partials
│   │   ├── Lists/              # Table cell partials (date, metric, pagination, actions)
│   │   ├── Badges/             # IATA and status badges
│   │   └── Kpi/                # Unified _KpiCard partial
│   └── Flight/                 # Domain partials (_FlightListPartial, _FlightStatusFilters)
├── wwwroot/
│   ├── css/site.css         # Custom styles on top of Bootstrap
│   └── js/                  # simulation.js (Cesium + polling loop), site.js, flight-create.js
├── FlightOps.Tests/          # xUnit unit + integration tests
├── FlightOps.E2E/             # Playwright end-to-end test
└── .github/workflows/         # ci.yml, azure-deploy.yml
```

---

## Simulation details

The `/Simulation` page:

1. Calls `GET /Simulation/GetActiveFlights` on load and every few seconds
2. The server runs `FlightLifecycleApplier` (auto-transitions), then returns all `Departed` flights whose window overlaps current UTC time (`TimeProvider`)
3. Each snapshot includes `CurrentLatitude`/`CurrentLongitude` (great-circle position), `Progress`, `FuelConsumed`, `RemainingSeconds`, and altitude (parabolic arc: `8000 + sin(π × progress) × 2000` m)
4. CesiumJS renders each aircraft as a moving entity on the 3D globe with a flight path polyline
5. Clicking a flight card opens the telemetry panel; a `POST /Simulation/CompleteFlight/{id}` button lands the flight manually

---

## Contributing

1. Fork / branch from `main`
2. `dotnet test --filter "Category!=E2E"` must pass before opening a PR
3. Follow existing conventions: feature folders, thin controllers, UTC timestamps, localized strings in RESX
4. Significant architecture changes → add an ADR in `docs/adr/`
5. See [`docs/onboarding.md`](docs/onboarding.md) for setup and codebase map

---

## About

Built by **João Magalhães** — full-stack .NET developer.

- Portfolio: [magalhaescode.netlify.app](https://magalhaescode.netlify.app/)
- LinkedIn: [linkedin.com/in/joaomagalhaespt](https://www.linkedin.com/in/joaomagalhaespt)
- Email: [jptmagalhaes2001@gmail.com](mailto:jptmagalhaes2001@gmail.com)
