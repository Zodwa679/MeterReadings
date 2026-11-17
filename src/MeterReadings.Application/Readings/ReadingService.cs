using MeterReadings.Application.Common;
using MeterReadings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Application.Readings;

public sealed class ReadingService(IApplicationDbContext db, TimeProvider timeProvider) : IReadingService
{
    public async Task<ReadingResponse> AddAsync(int meterId, CreateReadingRequest request, CancellationToken cancellationToken)
    {
        await EnsureMeterExistsAsync(meterId, cancellationToken);

        var timestampUtc = request.TimestampUtc!.Value;

        var reading = new MeterReading
        {
            MeterId = meterId,
            TimestampUtc = timestampUtc,
            CumulativeKwh = request.CumulativeKwh!.Value,
            IsReset = request.IsReset,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        db.MeterReadings.Add(reading);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(reading);
    }

    public async Task<IReadOnlyList<ReadingResponse>> GetAsync(
        int meterId, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken)
    {
        await EnsureMeterExistsAsync(meterId, cancellationToken);

        var query = db.MeterReadings.AsNoTracking().Where(r => r.MeterId == meterId);

        if (fromUtc is { } from)
        {
            query = query.Where(r => r.TimestampUtc >= from);
        }

        if (toUtc is { } to)
        {
            query = query.Where(r => r.TimestampUtc <= to);
        }

        var readings = await query.OrderBy(r => r.TimestampUtc).ToListAsync(cancellationToken);

        return readings.Select(ToResponse).ToList();
    }

    private async Task EnsureMeterExistsAsync(int meterId, CancellationToken cancellationToken)
    {
        if (!await db.Meters.AnyAsync(m => m.Id == meterId, cancellationToken))
        {
            throw new NotFoundException($"Meter {meterId} was not found.");
        }
    }

    private static ReadingResponse ToResponse(MeterReading r) =>
        new(r.Id, r.MeterId, r.TimestampUtc, r.CumulativeKwh, r.IsReset, r.CreatedAt);
}
