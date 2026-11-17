using MeterReadings.Domain.Entities;
using MeterReadings.Domain.Enums;
using MeterReadings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace MeterReadings.UnitTests.TestSupport;

public static class TestDb
{
    public static readonly DateTimeOffset Now = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);

    /// <summary>A clock frozen at <see cref="Now"/>, for services and validators that need the current time.</summary>
    public static FakeTimeProvider Clock() => new(Now);

    /// <summary>A fresh EF Core InMemory database (unique name per call).</summary>
    public static MeterReadingsDbContext Create()
    {
        var options = new DbContextOptionsBuilder<MeterReadingsDbContext>()
            .UseInMemoryDatabase($"meter-readings-{Guid.NewGuid()}")
            .Options;

        return new MeterReadingsDbContext(options);
    }

    public static async Task<Meter> AddMeterAsync(MeterReadingsDbContext db, string meterNumber)
    {
        var site = await db.Sites.FirstOrDefaultAsync();
        if (site is null)
        {
            site = new Site { Address = "1 Test Street", SiteType = SiteType.Residential, Status = SiteStatus.Active, CreatedAt = Now.UtcDateTime };
            db.Sites.Add(site);
        }

        var meter = new Meter
        {
            Site = site,
            MeterNumber = meterNumber,
            MeterType = MeterType.Electricity,
            Status = MeterStatus.Active,
            CreatedAt = Now.UtcDateTime
        };

        db.Meters.Add(meter);
        await db.SaveChangesAsync();
        return meter;
    }
}
