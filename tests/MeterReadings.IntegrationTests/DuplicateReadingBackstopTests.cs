using System.Net;
using FluentAssertions;
using MeterReadings.Application.Common;
using MeterReadings.Domain.Entities;
using MeterReadings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeterReadings.IntegrationTests;

/// <summary>
/// Exercises the database half of the duplicate rule against a real SQL Server: the unique index
/// IX_MeterReadings_MeterId_TimestampUtc and its translation into a ConflictException (409).
/// The EF Core InMemory provider used by the unit tests cannot prove this.
/// </summary>
[Collection(ApiCollection.Name)]
public class DuplicateReadingBackstopTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    private static readonly DateTime Timestamp = new(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task SaveChanges_DuplicateTimestampForSameMeter_ThrowsConflictException()
    {
        var meterId = await CreateMeterAsync();

        await InsertAsync(meterId, 100m);
        var act = () => InsertAsync(meterId, 120m);

        await act.Should().ThrowAsync<ConflictException>();

        using var scope = Fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MeterReadingsDbContext>();
        (await db.MeterReadings.CountAsync(r => r.MeterId == meterId)).Should().Be(1);
    }

    [Fact]
    public async Task PostReading_SameReadingSentConcurrently_ExactlyOneIsCreatedAndTheRestGet409()
    {
        var meterId = await CreateMeterAsync();

        var responses = await Task.WhenAll(
            Enumerable.Range(0, 10).Select(_ => PostReadingAsync(meterId, "2026-11-01T08:00:00Z", 100m)));

        responses.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(9);

        var stored = await ReadJsonAsync(await Client.GetAsync($"/api/meters/{meterId}/readings"));
        stored.GetArrayLength().Should().Be(1);
    }

    private async Task InsertAsync(int meterId, decimal kwh)
    {
        using var scope = Fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MeterReadingsDbContext>();

        db.MeterReadings.Add(new MeterReading
        {
            MeterId = meterId,
            TimestampUtc = Timestamp,
            CumulativeKwh = kwh,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
