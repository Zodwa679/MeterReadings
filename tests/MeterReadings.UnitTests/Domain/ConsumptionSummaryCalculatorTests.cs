using FluentAssertions;
using MeterReadings.Domain.Calculations;
using MeterReadings.Domain.Entities;
using MeterReadings.UnitTests.TestSupport;
using static MeterReadings.UnitTests.TestSupport.TestReadings;

namespace MeterReadings.UnitTests.Domain;

public class ConsumptionSummaryCalculatorTests
{
    private static readonly DateTime From = Utc("2026-11-01T00:00:00Z");
    private static readonly DateTime To = Utc("2026-11-03T00:00:00Z");

    private readonly ConsumptionSummaryCalculator _sut = new();

    private static DailyConsumption Day(int year, int month, int day, decimal kwh) => new(new DateOnly(year, month, day), kwh);

    [Fact]
    public void Calculate_MultiReadingMultiDay_ReturnsTotalAndDailyBreakdown()
    {
        var summary = _sut.Calculate(Meter1Seed(), From, To);

        summary.ReadingCount.Should().Be(5);
        summary.TotalKwh.Should().Be(81.9m);
        summary.Daily.Should().Equal(
            Day(2026, 11, 1, 21.4m),
            Day(2026, 11, 2, 38.8m),
            Day(2026, 11, 3, 21.7m));
        summary.Daily.Sum(d => d.Kwh).Should().Be(summary.TotalKwh);
    }

    [Fact]
    public void Calculate_IncreasingReadings_TotalEqualsLatestMinusEarliest()
    {
        // The brief defines total consumption as "latest reading minus earliest reading within the range".
        MeterReading[] readings =
        [
            At("2026-11-01T00:00:00Z", 1000.000m),
            At("2026-11-01T06:15:00Z", 1003.125m),
            At("2026-11-01T23:59:59Z", 1003.125m),
            At("2026-11-02T08:00:00Z", 1050.500m),
            At("2026-11-02T20:30:00Z", 1099.999m),
            At("2026-11-03T00:00:00Z", 1200.001m)
        ];

        var summary = _sut.Calculate(readings, From, To);

        var latestMinusEarliest = readings.MaxBy(r => r.TimestampUtc)!.CumulativeKwh
                                  - readings.MinBy(r => r.TimestampUtc)!.CumulativeKwh;
        summary.TotalKwh.Should().Be(latestMinusEarliest);

        var meter1 = Meter1Seed();
        _sut.Calculate(meter1, From, To).TotalKwh
            .Should().Be(meter1[^1].CumulativeKwh - meter1[0].CumulativeKwh);
    }

    [Fact]
    public void Calculate_ReadingsExactlyAtRangeBoundaries_AreIncluded()
    {
        MeterReading[] readings =
        [
            At("2026-11-01T00:00:00Z", 100m),
            At("2026-11-02T00:00:00Z", 110m),
            At("2026-11-03T00:00:00Z", 130m)
        ];

        var summary = _sut.Calculate(readings, From, To);

        summary.ReadingCount.Should().Be(3);
        summary.TotalKwh.Should().Be(30m);
    }

    [Fact]
    public void Calculate_ReadingsOutsideRange_AreExcluded()
    {
        MeterReading[] readings =
        [
            At("2026-10-31T23:59:59Z", 0m),
            At("2026-11-01T00:00:00Z", 100m),
            At("2026-11-02T00:00:00Z", 110m),
            At("2026-11-03T00:00:00Z", 130m),
            At("2026-11-03T00:00:01Z", 1000m)
        ];

        var summary = _sut.Calculate(readings, From, To);

        summary.ReadingCount.Should().Be(3);
        summary.TotalKwh.Should().Be(30m);
        summary.Daily.Should().Equal(Day(2026, 11, 2, 10m), Day(2026, 11, 3, 20m));
    }

    [Fact]
    public void Calculate_NoReadingsInRange_ReturnsZeroTotalAndEmptyDaily()
    {
        var summary = _sut.Calculate([At("2026-12-01T00:00:00Z", 5m)], From, To);

        summary.ReadingCount.Should().Be(0);
        summary.TotalKwh.Should().Be(0m);
        summary.Daily.Should().BeEmpty();
    }

    [Fact]
    public void Calculate_OneReadingInRange_ReturnsZeroTotalAndEmptyDaily()
    {
        var summary = _sut.Calculate([At("2026-11-02T00:00:00Z", 5m)], From, To);

        summary.ReadingCount.Should().Be(1);
        summary.TotalKwh.Should().Be(0m);
        summary.Daily.Should().BeEmpty();
    }

    [Fact]
    public void Calculate_UnorderedInput_IsOrderedBeforePairing()
    {
        var seed = Meter1Seed();
        MeterReading[] shuffled = [seed[3], seed[0], seed[4], seed[2], seed[1]];

        var summary = _sut.Calculate(shuffled, From, To);

        summary.TotalKwh.Should().Be(81.9m);
        summary.Daily.Should().Equal(
            Day(2026, 11, 1, 21.4m),
            Day(2026, 11, 2, 38.8m),
            Day(2026, 11, 3, 21.7m));
    }

    [Fact]
    public void Calculate_PairCrossingMidnight_IsAttributedToLaterReadingsDate()
    {
        MeterReading[] readings =
        [
            At("2026-11-01T23:00:00Z", 100m),
            At("2026-11-02T01:00:00Z", 104m)
        ];

        var summary = _sut.Calculate(readings, From, To);

        summary.Daily.Should().Equal(Day(2026, 11, 2, 4m));
    }

    [Fact]
    public void Calculate_RegisterReset_CountsUsageSinceTheResetAndNeverGoesNegative()
    {
        var summary = _sut.Calculate(Meter2Seed(), From, To);

        summary.ReadingCount.Should().Be(5);
        summary.TotalKwh.Should().Be(95.0m);
        summary.Daily.Should().Equal(
            Day(2026, 11, 1, 40.0m),
            Day(2026, 11, 2, 40.0m), // 15 since the reset + 25
            Day(2026, 11, 3, 15.0m));
        summary.Daily.Sum(d => d.Kwh).Should().Be(summary.TotalKwh);
    }

    [Fact]
    public void Calculate_UnexplainedDecrease_IsExcludedFromTotalAndDaily()
    {
        MeterReading[] readings =
        [
            At("2026-11-01T00:00:00Z", 500m),
            At("2026-11-01T12:00:00Z", 520m),
            At("2026-11-02T00:00:00Z", 300m), // unexplained drop — contributes 0
            At("2026-11-02T12:00:00Z", 310m)
        ];

        var summary = _sut.Calculate(readings, From, To);

        summary.TotalKwh.Should().Be(30m);
        summary.Daily.Should().Equal(Day(2026, 11, 1, 20m), Day(2026, 11, 2, 10m));
    }

    [Fact]
    public void Calculate_FractionalValues_AreRoundedToThreeDecimals()
    {
        MeterReading[] readings =
        [
            At("2026-11-01T00:00:00Z", 0m),
            At("2026-11-01T12:00:00Z", 1.00049m)
        ];

        var summary = _sut.Calculate(readings, From, To);

        summary.TotalKwh.Should().Be(1.000m);
        summary.Daily.Should().Equal(Day(2026, 11, 1, 1.000m));
    }
}
