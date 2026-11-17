using MeterReadings.Application.Common;
using MeterReadings.Domain.Entities;
using MeterReadings.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Application.Meters;

public sealed class MeterService(IApplicationDbContext db, TimeProvider timeProvider) : IMeterService
{
    public async Task<IReadOnlyList<MeterResponse>> GetBySiteAsync(int siteId, CancellationToken cancellationToken)
    {
        await EnsureSiteExistsAsync(siteId, cancellationToken);

        var meters = await db.Meters
            .AsNoTracking()
            .Where(m => m.SiteId == siteId)
            .OrderBy(m => m.Id)
            .ToListAsync(cancellationToken);

        return meters.Select(ToResponse).ToList();
    }

    public async Task<MeterResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var meter = await db.Meters.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
                    ?? throw new NotFoundException($"Meter {id} was not found.");

        return ToResponse(meter);
    }

    public async Task<MeterResponse> CreateAsync(int siteId, CreateMeterRequest request, CancellationToken cancellationToken)
    {
        await EnsureSiteExistsAsync(siteId, cancellationToken);

        var meterNumber = request.MeterNumber!.Trim();

        var meter = new Meter
        {
            SiteId = siteId,
            MeterNumber = meterNumber,
            MeterType = request.MeterType ?? MeterType.Electricity,
            Status = request.Status ?? MeterStatus.Active,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        db.Meters.Add(meter);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(meter);
    }

    private async Task EnsureSiteExistsAsync(int siteId, CancellationToken cancellationToken)
    {
        if (!await db.Sites.AnyAsync(s => s.Id == siteId, cancellationToken))
        {
            throw new NotFoundException($"Site {siteId} was not found.");
        }
    }

    private static MeterResponse ToResponse(Meter meter) =>
        new(meter.Id, meter.SiteId, meter.MeterNumber, meter.MeterType, meter.Status, meter.CreatedAt);
}
