# Meter Readings API

A REST API for capturing cumulative electricity meter readings and calculating consumption. Built with ASP.NET Core controllers, EF Core 8 and SQL Server on .NET 8. Design notes are in [SOLUTION.md](SOLUTION.md).

## Run

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and Docker.

```bash
docker compose up -d --wait   # SQL Server 2022 on localhost:1433
dotnet run --project src/MeterReadings.Api
```

Open <http://localhost:5271/swagger>. In Development the API applies the migration and seed data on startup. To use another server, set `ConnectionStrings__DefaultConnection`.

## Test

```bash
dotnet test                                # all tests; Docker must be running
dotnet test tests/MeterReadings.UnitTests  # unit tests only, no Docker
```

## Endpoints

| Method | Route | Body / query |
|---|---|---|
| POST, GET | `/api/sites` | `{ address, siteType?, status? }` |
| GET | `/api/sites/{id}` | |
| POST, GET | `/api/sites/{siteId}/meters` | `{ meterNumber, meterType?, status? }` |
| GET | `/api/meters/{id}` | |
| POST | `/api/meters/{meterId}/readings` | `{ timestampUtc, cumulativeKwh, isReset? }` |
| GET | `/api/meters/{meterId}/readings` | `?fromUtc=&toUtc=` optional, inclusive |
| GET | `/api/meters/{meterId}/summary` | `?fromUtc=&toUtc=` required, inclusive |

Timestamps must end in `Z` or an explicit offset such as `+02:00`; values without a zone return 400. Errors are ProblemDetails: 400 lists fields under `errors`, 404 and 409 explain the cause in `detail`.

## Examples

Seed data: site 1 with meters 1 (`MTR-1001`) and 2 (`MTR-1002`, includes a register reset). On a fresh database (`docker compose down -v`) the ids below line up.

```bash
# Create a site → 201, Location: /api/sites/2
curl -i -X POST localhost:5271/api/sites -H 'Content-Type: application/json' \
  -d '{"address":"12 Main Road, Cape Town","siteType":"Residential"}'

# Create a meter → 201, Location: /api/meters/3
curl -i -X POST localhost:5271/api/sites/2/meters -H 'Content-Type: application/json' \
  -d '{"meterNumber":"MTR-2001"}'

# Post a reading → 201
curl -i -X POST localhost:5271/api/meters/3/readings -H 'Content-Type: application/json' \
  -d '{"timestampUtc":"2026-11-01T08:00:00Z","cumulativeKwh":12543.7}'

# Same instant with an offset → 409 "A record with the same unique value already exists."
curl -i -X POST localhost:5271/api/meters/3/readings -H 'Content-Type: application/json' \
  -d '{"timestampUtc":"2026-11-01T10:00:00+02:00","cumulativeKwh":12543.7}'

# Consumption summary
curl "localhost:5271/api/meters/2/summary?fromUtc=2026-11-01T00:00:00Z&toUtc=2026-11-03T00:00:00Z"
```

```json
{
  "meterId": 2,
  "fromUtc": "2026-11-01T00:00:00Z",
  "toUtc": "2026-11-03T00:00:00Z",
  "readingCount": 5,
  "totalKwh": 95.000,
  "daily": [
    { "dateUtc": "2026-11-01", "kwh": 40.000 },
    { "dateUtc": "2026-11-02", "kwh": 40.000 },
    { "dateUtc": "2026-11-03", "kwh": 15.000 }
  ]
}
```
