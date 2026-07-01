# FlightOps — Navigation Map

Routes, user flows, and diagrams derived from the current codebase.

**See also:** [Requirements](requirements.md) · [Decision map](decisions-map.md) · [API](api.md)

---

## 1. Site map (authenticated)

```mermaid
flowchart TB
    subgraph Public["Public / anonymous"]
        Login["/Account/Login"]
        AccessDenied["/Account/AccessDenied"]
        Privacy["/Home/Privacy"]
        Health["/health"]
    end

    subgraph Nav["Main navbar"]
        Home["/ Home Dashboard"]
        Airports["/Airport"]
        Aircraft["/Aircraft"]
        Flights["/Flight"]
        Report["/Flight/Report"]
        Simulation["/Simulation"]
    end

    Login -->|success| Home
    Home --- Airports
    Home --- Aircraft
    Home --- Flights
    Home --- Report
    Home --- Simulation

    Airports --> AptIndex["Index"]
    Airports --> AptDetails["Details/{id}"]
    Airports --> AptCreate["Create ✎"]
    Airports --> AptEdit["Edit/{id} ✎"]

    Aircraft --> AcIndex["Index"]
    Aircraft --> AcDetails["Details/{id}"]
    Aircraft --> AcCreate["Create ✎"]
    Aircraft --> AcEdit["Edit/{id} ✎"]

    Flights --> FltIndex["Index"]
    Flights --> FltDetails["Details/{id}"]
    Flights --> FltCreate["Create ✎"]
    Flights --> FltEdit["Edit/{id} ✎"]
    Flights --> FltReportDetail["ReportDetail/{id}"]

    Simulation --> SimGlobe["3D Globe"]
    Simulation --> SimAPI["JSON poll"]

    classDef write fill:#3d2c02,stroke:#f0ad4e,color:#fff
    class AptCreate,AptEdit,AcCreate,AcEdit,FltCreate,FltEdit write
```

**Legend:** routes marked **✎** require the `Operator` role.

---

## 2. Authorization by area

```mermaid
flowchart LR
    subgraph ViewerOrOperator["Policy: ViewerOrOperator"]
        V1[Lists / Details]
        V2[Dashboard]
        V3[Report]
        V4[Simulation view]
        V5[API GET]
        V6[CalculatePreview]
    end

    subgraph OperatorOnly["Policy: OperatorOnly"]
        O1[Create / Edit / Delete]
        O2[CompleteFlight]
    end

    User((User)) --> Auth{Authenticated?}
    Auth -->|no| Login
    Auth -->|yes| Role{Role}
    Role --> ViewerOrOperator
    Role -->|Operator| OperatorOnly
```

| Controller | GET read | POST write |
|------------|----------|------------|
| `HomeController` | Authenticated | — |
| `AccountController` | Anonymous (login) | Logout authenticated |
| `AirportController` | ViewerOrOperator | OperatorOnly |
| `AircraftController` | ViewerOrOperator | OperatorOnly |
| `FlightController` | ViewerOrOperator | OperatorOnly |
| `SimulationController` | ViewerOrOperator | CompleteFlight: OperatorOnly |
| `FlightsApiController` | ViewerOrOperator | — |

---

## 3. Flow: create airport

```mermaid
sequenceDiagram
    actor Op as Operator
    participant UI as Airport/Create
    participant AC as AirportController
    participant Repo as AirportRepository
    participant DB as SQLite

    Op->>UI: Fill form
    UI->>AC: POST Create (Anti-forgery)
    AC->>AC: ModelState (Data Annotations)
    alt invalid
        AC-->>UI: View with errors
    else valid
        AC->>Repo: CreateAirportAsync
        Repo->>DB: INSERT
        alt duplicate IATA
            DB-->>AC: Unique violation
            AC-->>UI: Error.DuplicateAirportIata
        else OK
            AC-->>Op: Redirect Index
        end
    end
```

**Prerequisites:** none.

**Fields:** Name, City, Country, IATA (3 chars), Latitude, Longitude.

---

## 4. Flow: create aircraft

```mermaid
sequenceDiagram
    actor Op as Operator
    participant UI as Aircraft/Create
    participant AC as AircraftController
    participant AR as AirportRepository
    participant Repo as AircraftRepository
    participant DB as SQLite

    Op->>UI: GET Create
    AC->>AR: GetAllAirportsAsync
    AR-->>UI: Home airport dropdown

    Op->>UI: Fill + POST
    AC->>AC: ModelState
    AC->>Repo: CreateAircraftAsync
    Repo->>DB: INSERT
    alt duplicate registration
        DB-->>AC: Unique violation
        AC-->>UI: Error.DuplicateAircraftRegistration
    else OK
        AC-->>Op: Redirect Index
    end
```

**Prerequisites:**

1. ≥ 1 airport (for `CurrentAirportId`)
2. Operator role

**Required fields:** Registration, Name, Model, Home airport, TakeOffEffort ≥ 1, FuelConsumptionPerKm > 0, CruiseSpeedKmh > 0.

---

## 5. Flow: create flight

```mermaid
sequenceDiagram
    actor Op as Operator
    participant UI as Flight/Create
    participant FC as FlightController
    participant Cmd as FlightCommands
    participant Life as FlightLifecycleApplier
    participant Calc as FlightCalculatorService
    participant Lock as AircraftBookingLock
    participant Val as FlightScheduleValidator
    participant Loc as AircraftLocationResolver
    participant Repo as FlightRepository
    participant Hang as HangarLocationSynchronizer

    Op->>UI: Select origin, destination, aircraft, time
    UI->>FC: GET CalculatePreview (debounced)
    FC-->>UI: distance, fuel, arrival

    Op->>UI: POST Create
    FC->>Cmd: CreateFlightAsync
    Cmd->>Life: ApplyTransitionsAsync
    Cmd->>Cmd: Reject if origin = destination
    Cmd->>Calc: Haversine + fuel + ETA
    Cmd->>Lock: AcquireAsync(aircraftId)
    Cmd->>Val: ValidateAsync
    Val->>Loc: ResolveAirportIdAsync
    alt conflict or wrong origin
        Val-->>Cmd: AircraftScheduleConflict / AircraftWrongOrigin
        Cmd-->>UI: Localized error
    else OK
        Cmd->>Repo: CreateFlightAsync
        Cmd->>Hang: SyncHangarLocationsAsync
        Cmd-->>Op: Redirect
    end
```

**Prerequisites (recommended order):**

```mermaid
flowchart TD
    S1["1. Create airports (min. 2 distinct)"]
    S2["2. Create aircraft with home base"]
    S3["3. Verify aircraft available at origin at chosen time"]
    S4["4. Verify no schedule conflict"]
    S5["5. Submit flight Scheduled or Departed"]

    S1 --> S2 --> S3 --> S4 --> S5
```

**Allowed statuses on create:** `Scheduled`, `Departed`.

**Server-computed fields:** Distance, Fuel, ArrivalTime (do not trust the client).

---

## 6. Flight lifecycle

```mermaid
stateDiagram-v2
    [*] --> Scheduled: Create
    Scheduled --> Departed: Departure time reached\nor manual dispatch
    Scheduled --> Cancelled: now ≥ ArrivalTime\n(missed flight)
    Scheduled --> Cancelled: Manual cancel
    Departed --> Arrived: now ≥ ArrivalTime\nor CompleteFlight
    Arrived --> [*]
    Cancelled --> [*]

    note right of Scheduled
        Edit allowed
        (except after Arrived/Cancelled)
    end note
    note right of Departed
        Origin and aircraft locked
    end note
```

---

## 7. Simulation and monitoring

```mermaid
flowchart TB
    subgraph Dashboard["/ Home"]
        KPI[KPI cards]
        Next[Upcoming departures]
        Fleet[Fleet status]
    end

    subgraph Live["/Simulation"]
        Poll["poll GetActiveFlights"]
        Globe[CesiumJS globe]
        Panel[Progress/fuel panel]
    end

    subgraph Background["Background 60s"]
        BGS[FlightLifecycleBackgroundService]
        BGS --> Trans[ApplyLifecycleTransitions]
        Trans --> Hang[SyncHangarLocations]
    end

    Dashboard --> Live
    Trans --> Dashboard
```

---

## 8. REST API

```mermaid
flowchart LR
    Client[HTTP Client] -->|Identity cookie| API["/api/flights"]
    API --> List["GET /"]
    API --> ById["GET /{id}"]
    API --> Active["GET /active"]
```

Interactive docs: `/swagger` (Development only). See [api.md](api.md).

---

## 9. Navbar utilities

| Element | Route / action |
|---------|----------------|
| UTC clock | `_NavbarUtils` |
| Language | `GET /Home/SetCulture?culture=&returnUrl=` |
| Login / Logout | `/Account/Login`, `POST /Account/Logout` |

Valid cultures: `en`, `pt-PT`, `de-DE`.

---

## 10. Typical operational onboarding order

1. Log in as `operator@flightops.demo`
2. Review seeded airports at `/Airport`
3. Review fleet at `/Aircraft` (or create new with home base)
4. Create flight at `/Flight/Create` — use preview before submit
5. Monitor at `/` (dashboard) or `/Simulation`
6. History at `/Flight/Report`

For external integration: consume `GET /api/flights/active` with an authenticated session.
