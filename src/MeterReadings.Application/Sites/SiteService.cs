using MeterReadings.Application.Common;
using MeterReadings.Domain.Entities;
using MeterReadings.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Application.Sites;

public sealed class SiteService(IApplicationDbContext db, TimeProvider timeProvider) : ISiteService
{
    public async Task<IReadOnlyList<SiteResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var sites = await db.Sites
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToListAsync(cancellationToken);

        return sites.Select(ToResponse).ToList();
    }

    public async Task<SiteResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var site = await db.Sites.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
                   ?? throw new NotFoundException($"Site {id} was not found.");

        return ToResponse(site);
    }

    public async Task<SiteResponse> CreateAsync(CreateSiteRequest request, CancellationToken cancellationToken)
    {
        var site = new Site
        {
            Address = request.Address!.Trim(),
            SiteType = request.SiteType ?? SiteType.Unspecified,
            Status = request.Status ?? SiteStatus.Active,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        db.Sites.Add(site);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(site);
    }

    private static SiteResponse ToResponse(Site site) =>
        new(site.Id, site.Address, site.SiteType, site.Status, site.CreatedAt);
}
