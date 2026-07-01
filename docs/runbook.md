# FlightOps — Operations runbook

When to use this document: deploying, monitoring, or troubleshooting a running FlightOps instance (local Docker, Azure App Service, or manual host).

---

## Prerequisites & access

| Requirement | Details |
|-------------|---------|
| Azure App Service | Linux, .NET 9 runtime; publish profile or GitHub Actions secrets |
| GitHub secrets | `AZURE_WEBAPP_NAME`, `AZURE_WEBAPP_PUBLISH_PROFILE` |
| App settings | `ConnectionStrings__DefaultConnection`, `ASPNETCORE_ENVIRONMENT` |
| Health probe access | `GET /health` is anonymous |

**Single-instance assumption:** the in-process `AircraftBookingLock` is not safe across multiple app instances. See [ADR-006](adr/006-single-instance-deployment.md).

---

## Health checks

### `GET /health`

- **Auth:** none required
- **Checks:** EF Core can connect to SQLite (`AddDbContextCheck`)
- **Healthy:** `200` with `"Healthy"` status
- **Unhealthy:** `503` if database unreachable

```bash
curl -s https://<host>/health
```

**Azure App Service:** Configuration → Health check → path `/health`.

### Application logs

Every non-static request is logged by `RequestLoggingMiddleware`:

```
{Method} {Path} responded {StatusCode} in {ElapsedMs}ms ({CorrelationId})
```

Pass `X-Correlation-ID` on requests to tie client logs to server logs.

### Background lifecycle service

`FlightLifecycleBackgroundService` runs every **60 seconds**:

- Transitions overdue `Scheduled` flights → `Cancelled`
- Transitions `Departed` → `Arrived` when arrival time passes
- Syncs aircraft hangar locations after transitions

Failures per tick are logged; the host process continues.

---

## Deployment procedure

### CI/CD (recommended)

Push to `main`:

1. **CI** (`.github/workflows/ci.yml`) — restore, build, test (excludes E2E)
2. **Azure Deploy** (`.github/workflows/azure-deploy.yml`) — test gate, then `dotnet publish` + `azure/webapps-deploy`

Deploy only proceeds if tests pass.

### Manual Azure setup (one-time)

1. Create App Service (Linux, .NET 9) — F1 tier works for demo traffic.
2. Application settings:
   - `ConnectionStrings__DefaultConnection` = `Data Source=/home/flightops.db`
   - `ASPNETCORE_ENVIRONMENT` = `Production`
3. Health check path: `/health`
4. Configure GitHub secrets for automated deploy (see [README](../README.md#deploying-to-azure-app-service)).

### Docker

```bash
docker compose up --build -d
```

- Port: `8080`
- DB file: `./data/flightops.db` (volume-mounted)
- Migrations + seed run on container start

---

## Rollback

### Azure (GitHub Actions deploy)

1. Identify last good commit on `main`.
2. Revert or cherry-pick fix, push to `main` — deploy workflow re-runs automatically.
3. **Or** redeploy a previous artifact from Azure Portal → Deployment Center → Logs.

SQLite data in `/home/flightops.db` survives redeploys unless the file is deleted manually.

### Docker

```bash
docker compose down          # container stops; ./data/flightops.db kept
git checkout <good-commit>
docker compose up --build -d
```

### Database rollback

There is no automated down-migration in CI. For schema rollback:

1. Stop the app.
2. Restore `flightops.db` from backup (or delete for fresh seed on next start — **data loss**).
3. Ensure migration history matches the deployed code version.

---

## Troubleshooting

### App won't start — migration failure

**Symptoms:** crash loop on startup, EF migration exception in logs.

**Fix (dev/demo):** stop app, delete SQLite file, restart (migrations re-applied, data re-seeded).

**Fix (prod with data):** restore DB backup; align deployed code version with migration history.

### `/health` returns 503

**Cause:** SQLite file missing, wrong path, or permissions on `/home`.

**Check:**
- App setting `ConnectionStrings__DefaultConnection`
- Azure: `/home` is writable on Linux App Service
- Docker: volume mount `./data:/app/data` exists

### Flights stuck in wrong status

**Cause:** background service not running, or clock skew.

**Check:**
- Logs for `FlightLifecycleBackgroundService` errors
- Server uses UTC via `TimeProvider.System`
- Manual trigger: any mutating flight request also runs `FlightLifecycleApplier`

**Manual fix (Operator UI):** edit flight status or use Simulation → Complete Flight for departed flights.

### Double-booking / schedule conflicts

**Expected:** `FlightScheduleValidator` rejects overlapping bookings with a validation error.

**If suspected race:** app must be single-instance (see ADR-002/006). Multiple instances bypass `AircraftBookingLock`.

### 502/503 after deploy on Azure

1. Check `/health`
2. Check App Service logs (Log stream or Kudu)
3. Verify .NET 9 runtime stack selected
4. Verify `ASPNETCORE_ENVIRONMENT=Production` (Swagger disabled; exception handler enabled)

### E2E / smoke test after deploy

```bash
curl -s -o /dev/null -w "%{http_code}" https://<host>/health
# Login via browser with demo credentials
# GET /api/flights (authenticated) should return 200 + JSON array
```

---

## Configuration reference

| Setting | Default (local) | Production (Azure) |
|---------|-----------------|---------------------|
| `ConnectionStrings__DefaultConnection` | `Data Source=flightops.db` | `Data Source=/home/flightops.db` |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `PLAYWRIGHT_BASE_URL` | — | E2E only, not production |

---

## Escalation

This is a portfolio/demo app with a single maintainer.

| Issue | Action |
|-------|--------|
| Code bug | Open GitHub issue or fix + PR |
| Azure platform outage | [Azure Status](https://status.azure.com/) |
| Data loss | Restore from last known `flightops.db` backup |

Contact: [jptmagalhaes2001@gmail.com](mailto:jptmagalhaes2001@gmail.com)
