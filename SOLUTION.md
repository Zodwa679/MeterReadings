# Solution notes

## Architecture

```
Api ──► Application ──► Domain
 │           ▲
 └──► Infrastructure
```

| Project | Responsibility |
|---|---|
| **Domain** | Entities, enums and the pure calculators (`UsageCalculator`, `ConsumptionSummaryCalculator`). No dependencies. |
| **Application** | Services and DTOs per feature; defines `IApplicationDbContext`. |
| **Infrastructure** | `MeterReadingsDbContext`, entity configurations, the migration and seed data. |
| **Api** | Controllers, exception handling, UTC timestamp parsing. |

**Data model:** a site has many meters, and a meter has many readings. A unique index on `(MeterId, TimestampUtc)` enforces one reading per meter per instant; `CumulativeKwh` is `decimal(12,3)` with a check constraint `>= 0`.

Diagrams: [architecture](docs/diagrams/architecture-layers.png), [ERD](docs/diagrams/erd.png), [post a reading](docs/diagrams/flow-post-reading.png), [summary](docs/diagrams/flow-summary.png).

## Key decisions

| Decision | Why |
|---|---|
| **Controllers**, not minimal APIs | `[ApiController]` gives automatic model validation and 400 ProblemDetails, and is a familiar convention for .NET teams. |
| **Services + `IApplicationDbContext`**, no MediatR or repositories | Nine endpoints don't need a handler per operation; `DbContext` is already a unit of work. |
| **Data annotations** for validation | `[Required]`, `[MaxLength]` and `[Range]` cover this scope with no extra library. |
| **Strict UTC parsing** for `fromUtc`/`toUtc` and request bodies | The default binder converts offsets to server-local time and accepts values with no zone; `UtcDateTimeParser` rejects zone-less values and normalises offsets to UTC. |
| **Total = latest − earliest**, as the brief specifies | Decreasing readings are optional in the brief. If the range contains a reset or decrease, the total is the sum of corrected pair differences instead: for seeded meter 2, latest − earliest is −99,895 kWh, the corrected total is 95 kWh. |
| **Resets via an `IsReset` flag** | A flagged reading's usage is its own value (counting from zero); an unflagged decrease contributes 0 kWh. |
| **Duplicates enforced by the unique index only** | The index stays correct under concurrent inserts, which an "exists?" pre-check does not. SQL errors 2601/2627 become a 409 with a generic message. |
| **Only `address` and `meterNumber` required** | `siteType`, `meterType` and `status` default to `Unspecified`, `Electricity` and `Active`. |
| **Default validation error keys** | Annotation errors use property names (`Address`), JSON errors use paths (`$.timestampUtc`), as ASP.NET Core produces them. |

**Future options:** field-specific 409 messages, anomaly reporting for unflagged decreases, rejecting readings for inactive meters, camelCase error keys, update/delete, pagination, authentication.

## Testing

- **Unit tests** (no database): the calculators, covering a multi-day breakdown, inclusive boundaries, midnight-crossing pairs, unordered input, resets and rounding; plus InMemory service tests for not-found and ordering.
- **Integration tests** (`WebApplicationFactory` + Testcontainers SQL Server): routing, status codes, ProblemDetails and UTC handling. The duplicate rule is tested here because InMemory doesn't enforce unique indexes: a direct duplicate insert, the same instant with a different offset, and 10 concurrent identical requests producing exactly one 201 and nine 409s.

## AI use

I designed the data model on a whiteboard and used **Claude (chat)** to review it and turn the agreed design into a specification. I then built the solution with **Claude Code** as a copilot: I set the architecture, constraints and test cases and directed each change. Decisions I made along the way included using controllers instead of minimal APIs, data annotations instead of FluentValidation, and the database unique index as the only duplicate check. Any code produced by AI was reviewed, tested and owned by me.
| Suggestion | Decision | Why |
|---|---|---|
| Unique timestamp per meter, not globally | Accepted | Two meters can both be read at 08:00. |
| Unique meter number | Accepted | It is the meter's real-world identifier. |
| Real-database integration tests | Accepted | Only SQL Server can prove the unique index. |
| Reset flag with anomaly reporting | Changed | Kept the reset flag, left anomaly reporting as a future option. |
| Sum of pairs as the total everywhere | Changed | Kept the brief's formula; sum of pairs only after a reset or decrease. |
| Application-level duplicate pre-check | Rejected | Redundant with the index and unsafe under concurrency. |
| FluentValidation, MediatR, minimal APIs | Rejected | See Key decisions. |
| `Meter_Installation` table; peak/off-peak values | Rejected | Beyond the brief. |

**Checking correctness:** I calculated the expected summaries (81.9 kWh for meter 1, 95.0 kWh for meter 2) by hand from the seed readings before writing the tests, and exercised invalid input in Swagger (duplicate reading, missing zone, negative kWh) against the Docker database to confirm the 400 and 409 responses.
