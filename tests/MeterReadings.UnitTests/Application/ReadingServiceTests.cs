using FluentAssertions;
using MeterReadings.Application.Common;
using MeterReadings.Application.Readings;
using MeterReadings.UnitTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using static MeterReadings.UnitTests.TestSupport.TestReadings;

namespace MeterReadings.UnitTests.Application;

// The InMemory provider does not enforce unique indexes, so the duplicate-timestamp rule is tested
// against real SQL Server in MeterReadings.IntegrationTests (DuplicateReadingBackstopTests).
public class ReadingServiceTests
{
    private static CreateReadingRequest Request(DateTime timestampUtc, decimal kwh) => new(timestampUtc, kwh);

    [Fact]
    public async Task AddAsync_SameTimestampOnDifferentMeter_IsAccepted()
    {
        await using var db = TestDb.Create();
        var meterA = await TestDb.AddMeterAsync(db, "MTR-A");
        var meterB = await TestDb.AddMeterAsync(db, "MTR-B");
        var sut = new ReadingService(db, TestDb.Clock());
        var timestamp = Utc("2026-11-01T08:00:00Z");

        await sut.AddAsync(meterA.Id, Request(timestamp, 100m), CancellationToken.None);
        var act = () => sut.AddAsync(meterB.Id, Request(timestamp, 200m), CancellationToken.None);

        await act.Should().NotThrowAsync();
        (await db.MeterReadings.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task AddAsync_MissingMeter_ThrowsNotFound()
    {
        await using var db = TestDb.Create();
        var sut = new ReadingService(db, TestDb.Clock());

        var act = () => sut.AddAsync(999, Request(Utc("2026-11-01T08:00:00Z"), 1m), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddAsync_ValidReading_SetsCreatedAtFromTimeProvider()
    {
        await using var db = TestDb.Create();
        var meter = await TestDb.AddMeterAsync(db, "MTR-1");
        var sut = new ReadingService(db, TestDb.Clock());

        var reading = await sut.AddAsync(meter.Id, Request(Utc("2026-11-01T08:00:00Z"), 1m), CancellationToken.None);

        reading.CreatedAt.Should().Be(TestDb.Now.UtcDateTime);
        reading.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task GetAsync_ReadingsAddedOutOfOrder_ReturnsThemInTimestampOrder()
    {
        await using var db = TestDb.Create();
        var meter = await TestDb.AddMeterAsync(db, "MTR-1");
        var sut = new ReadingService(db, TestDb.Clock());

        foreach (var (timestamp, kwh) in new[]
                 {
                     ("2026-11-02T00:00:00Z", 30m),
                     ("2026-11-01T00:00:00Z", 10m),
                     ("2026-11-03T00:00:00Z", 40m),
                     ("2026-11-01T12:00:00Z", 20m)
                 })
        {
            await sut.AddAsync(meter.Id, Request(Utc(timestamp), kwh), CancellationToken.None);
        }

        var all = await sut.GetAsync(meter.Id, null, null, CancellationToken.None);
        var ranged = await sut.GetAsync(
            meter.Id, Utc("2026-11-01T12:00:00Z"), Utc("2026-11-02T00:00:00Z"), CancellationToken.None);

        all.Select(r => r.CumulativeKwh).Should().Equal(10m, 20m, 30m, 40m);
        all.Should().BeInAscendingOrder(r => r.TimestampUtc);
        ranged.Select(r => r.CumulativeKwh).Should().Equal(20m, 30m);
    }
}
