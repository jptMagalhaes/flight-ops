# FlightOps REST API

Read-only JSON API for flights. All endpoints require an authenticated session (ASP.NET Core Identity cookie). There is no bearer-token or API-key auth.

For interactive exploration, run the app in **Development** and open `/swagger`.

---

## Authentication

| Method | Details |
|--------|---------|
| Scheme | Cookie (`Identity.Application`) |
| Login | `POST /Account/Login` with form fields `Email`, `Password`, and anti-forgery token |
| Required role | `Viewer` or `Operator` (`ViewerOrOperator` policy) |
| Unauthenticated | `302` redirect to `/Account/Login` (browser) or `401`/`403` depending on client |

**Demo accounts** (seeded on first run):

| Role | Email | Password |
|------|-------|----------|
| Operator | `operator@flightops.demo` | `Operator123!` |
| Viewer | `viewer@flightops.demo` | `Viewer123!` |

### Example: authenticated curl session

```bash
# 1. Fetch login page and extract anti-forgery token (simplified — use a cookie jar)
curl -c cookies.txt -b cookies.txt https://localhost:7xxx/Account/Login

# 2. POST login (replace __RequestVerificationToken with value from step 1)
curl -c cookies.txt -b cookies.txt -X POST https://localhost:7xxx/Account/Login \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "Email=viewer@flightops.demo&Password=Viewer123!&__RequestVerificationToken=..."

# 3. Call API with session cookie
curl -b cookies.txt https://localhost:7xxx/api/flights
```

In Playwright E2E tests, login via the UI and reuse `APIRequest` with the same browser context — see `FlightOps.E2E/FlightApiTests.cs`.

---

## Base URL

| Environment | URL |
|-------------|-----|
| Local (`dotnet run`) | `https://localhost:<port>` or `http://localhost:5198` |
| Docker Compose | `http://localhost:8080` |
| Azure App Service | `https://<app-name>.azurewebsites.net` |

---

## Endpoints

All routes are under `Controllers/Api/FlightsApiController.cs`.

### `GET /api/flights`

Returns all flights, newest first (repository order).

**Response:** `200 OK` — JSON array of `FlightModel`

```json
[
  {
    "id": 1,
    "distance": 321.4,
    "fuel": 1164.2,
    "departureTime": "2026-07-01T10:00:00Z",
    "arrivalTime": "2026-07-01T10:24:00Z",
    "status": 0,
    "originId": 1,
    "destinationId": 2,
    "aircraftId": 3,
    "aircraft": { "id": 3, "registration": "CS-TUI", "name": "...", "model": "..." },
    "origin": { "id": 1, "name": "Lisbon", "city": "Lisbon", "country": "Portugal", "iata": "LIS", "latitude": 38.77, "longitude": -9.13 },
    "destination": { "id": 2, "name": "Porto", "city": "Porto", "country": "Portugal", "iata": "OPO", "latitude": 41.24, "longitude": -8.68 }
  }
]
```

---

### `GET /api/flights/{id}`

Returns a single flight by ID.

| Status | Condition |
|--------|-----------|
| `200 OK` | Flight found |
| `404 Not Found` | No flight with that ID |

---

### `GET /api/flights/active`

Returns live simulation snapshots for all **Departed** flights whose departure–arrival window overlaps current UTC time. The server runs lifecycle transitions before building the response.

**Response:** `200 OK` — JSON array of `ActiveFlightSimulationModel`

```json
[
  {
    "id": 5,
    "originIata": "LIS",
    "originName": "Lisbon",
    "destinationIata": "OPO",
    "destinationName": "Porto",
    "originLatitude": 38.77,
    "originLongitude": -9.13,
    "destinationLatitude": 41.24,
    "destinationLongitude": -8.68,
    "aircraftName": "CS-TUI",
    "aircraftModel": "A320neo",
    "distanceKm": 321.4,
    "totalFuel": 1164.2,
    "takeOffFuel": 200,
    "fuelConsumed": 580.1,
    "fuelBurnRatePerHour": 2400,
    "progress": 0.52,
    "currentLatitude": 40.1,
    "currentLongitude": -8.9,
    "currentAltitudeM": 9200,
    "remainingSeconds": 690,
    "elapsedSeconds": 750,
    "departureTime": "2026-07-01T10:00:00Z",
    "arrivalTime": "2026-07-01T10:24:00Z"
  }
]
```

---

## Data types

### `FlightStatus` (enum, serialized as integer)

| Value | Name |
|-------|------|
| `0` | Scheduled |
| `1` | Departed |
| `2` | Arrived |
| `3` | Cancelled |

### `FlightModel`

| Field | Type | Notes |
|-------|------|-------|
| `id` | int | Primary key |
| `distance` | double | Great-circle distance (km) |
| `fuel` | double | Total fuel (L) including take-off effort |
| `departureTime` | datetime (UTC) | ISO 8601 |
| `arrivalTime` | datetime (UTC) | Computed on create/update |
| `status` | int | `FlightStatus` |
| `originId`, `destinationId`, `aircraftId` | int | Foreign keys |
| `origin`, `destination`, `aircraft` | object | Nested models (see source in `Models/Forms/`) |

---

## Error responses

This API is read-only. Typical errors:

| HTTP | Cause |
|------|-------|
| `401` / redirect | No valid session cookie |
| `403` | Authenticated but missing `Viewer`/`Operator` role |
| `404` | Flight ID not found (`GET /api/flights/{id}` only) |

Mutating operations (create/edit/delete flights) are available only through the MVC UI (`/Flight/*`) and require the `Operator` role.

---

## Rate limits & pagination

None. The API returns full result sets. For large datasets, prefer the paginated MVC report at `/Flight/Report`.

---

## Related MVC endpoints (not REST)

These power the simulation UI and are **not** under `/api/`:

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/Simulation/GetActiveFlights` | Same data as `/api/flights/active` (JSON for Cesium polling) |
| POST | `/Simulation/CompleteFlight/{id}` | Manually land a departed flight (Operator only) |

---

## SDK / client notes

- Use cookie auth or automate login — no OAuth/JWT.
- Send optional `X-Correlation-ID` header; the server echoes it on every response for log correlation.
- Timestamps are UTC. The MVC UI converts to browser-local time; API consumers should do the same.
