using MeterReadings.Application.Common;
using MeterReadings.Domain.Entities;

namespace MeterReadings.UnitTests.TestSupport;

public static class TestReadings
{
    /// <summary>Builds a reading; the timestamp is parsed with the production UtcDateTimeParser.</summary>
    public static MeterReading At(string timestamp, decimal cumulativeKwh, bool isReset = false, int meterId = 1) =>
        new()
        {
            MeterId = meterId,
            TimestampUtc = Utc(timestamp),
            CumulativeKwh = cumulativeKwh,
            IsReset = isReset
        };

    public static DateTime Utc(string timestamp) =>
        UtcDateTimeParser.TryParse(timestamp, out var utc)
            ? utc
            : throw new ArgumentException($"Invalid test timestamp '{timestamp}'.", nameof(timestamp));

    /// <summary>Meter 1's seed readings: normal, increasing.</summary>
    public static MeterReading[] Meter1Seed() =>
    [
        At("2026-11-01T00:00:00Z", 12500.0m),
        At("2026-11-01T12:00:00Z", 12521.4m),
        At("2026-11-02T00:00:00Z", 12543.7m),
        At("2026-11-02T12:00:00Z", 12560.2m),
        At("2026-11-03T00:00:00Z", 12581.9m)
    ];

    /// <summary>Meter 2's seed readings: the register is reset to zero partway through.</summary>
    public static MeterReading[] Meter2Seed() =>
    [
        At("2026-11-01T00:00:00Z", 99950.0m, meterId: 2),
        At("2026-11-01T12:00:00Z", 99990.0m, meterId: 2),
        At("2026-11-02T00:00:00Z", 15.0m, isReset: true, meterId: 2),
        At("2026-11-02T12:00:00Z", 40.0m, meterId: 2),
        At("2026-11-03T00:00:00Z", 55.0m, meterId: 2)
    ];
}
