using FluentAssertions;
using MeterReadings.Application.Common;
using MeterReadings.Application.Summaries;
using MeterReadings.Domain.Calculations;
using MeterReadings.UnitTests.TestSupport;
using static MeterReadings.UnitTests.TestSupport.TestReadings;

namespace MeterReadings.UnitTests.Application;

public class SummaryServiceTests
{
    private static readonly SummaryQuery Range = new(Utc("2026-11-01T00:00:00Z"), Utc("2026-11-03T00:00:00Z"));

    private static SummaryService CreateSut(IApplicationDbContext db) =>
        new(db, new ConsumptionSummaryCalculator());

    [Fact]
    public async Task GetAsync_MissingMeter_ThrowsNotFound()
    {
        await using var db = TestDb.Create();
        var sut = CreateSut(db);

        var act = () => sut.GetAsync(999, Range, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAsync_SeededMeter2_ReturnsCalculatorResult()
    {
        // Smoke test: the calculation rules themselves are covered by the Domain tests.
        await using var db = TestDb.Create();
        await db.Database.EnsureCreatedAsync(); // applies HasData seed
        var sut = CreateSut(db);

        var summary = await sut.GetAsync(2, Range, CancellationToken.None);

        summary.MeterId.Should().Be(2);
        summary.FromUtc.Should().Be(Range.FromUtc);
        summary.ToUtc.Should().Be(Range.ToUtc);
        summary.ReadingCount.Should().Be(5);
        summary.TotalKwh.Should().Be(95.0m);
        summary.Daily.Should().Equal(
            new DailyConsumptionResponse(new DateOnly(2026, 11, 1), 40.0m),
            new DailyConsumptionResponse(new DateOnly(2026, 11, 2), 40.0m),
            new DailyConsumptionResponse(new DateOnly(2026, 11, 3), 15.0m));
    }
}
