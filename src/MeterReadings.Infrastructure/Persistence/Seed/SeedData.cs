using MeterReadings.Domain.Entities;
using MeterReadings.Domain.Enums;

namespace MeterReadings.Infrastructure.Persistence.Seed;

internal static class SeedData
{
    private static readonly DateTime SeededAt = Utc(2026, 11, 1, 0);

    public static readonly Site[] Sites =
    [
        new()
        {
            Id = 1,
            Address = "1 Demo Street, Johannesburg",
            SiteType = SiteType.Commercial,
            Status = SiteStatus.Active,
            CreatedAt = SeededAt
        }
    ];

    public static readonly Meter[] Meters =
    [
        new()
        {
            Id = 1,
            MeterNumber = "MTR-1001",
            SiteId = 1,
            MeterType = MeterType.Electricity,
            Status = MeterStatus.Active,
            CreatedAt = SeededAt
        },
        new()
        {
            Id = 2,
            MeterNumber = "MTR-1002",
            SiteId = 1,
            MeterType = MeterType.Electricity,
            Status = MeterStatus.Active,
            CreatedAt = SeededAt
        }
    ];

    public static readonly MeterReading[] MeterReadings =
    [
        Reading(1, 1, Utc(2026, 11, 1, 0), 12500.0m),
        Reading(2, 1, Utc(2026, 11, 1, 12), 12521.4m),
        Reading(3, 1, Utc(2026, 11, 2, 0), 12543.7m),
        Reading(4, 1, Utc(2026, 11, 2, 12), 12560.2m),
        Reading(5, 1, Utc(2026, 11, 3, 0), 12581.9m),

        Reading(6, 2, Utc(2026, 11, 1, 0), 99950.0m),
        Reading(7, 2, Utc(2026, 11, 1, 12), 99990.0m),
        Reading(8, 2, Utc(2026, 11, 2, 0), 15.0m, isReset: true),
        Reading(9, 2, Utc(2026, 11, 2, 12), 40.0m),
        Reading(10, 2, Utc(2026, 11, 3, 0), 55.0m)
    ];

    private static MeterReading Reading(long id, int meterId, DateTime timestampUtc, decimal kwh, bool isReset = false) =>
        new()
        {
            Id = id,
            MeterId = meterId,
            TimestampUtc = timestampUtc,
            CumulativeKwh = kwh,
            IsReset = isReset,
            CreatedAt = timestampUtc
        };

    private static DateTime Utc(int year, int month, int day, int hour) =>
        new(year, month, day, hour, 0, 0, DateTimeKind.Utc);
}
