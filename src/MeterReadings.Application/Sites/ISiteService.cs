namespace MeterReadings.Application.Sites;

public interface ISiteService
{
    Task<IReadOnlyList<SiteResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<SiteResponse> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<SiteResponse> CreateAsync(CreateSiteRequest request, CancellationToken cancellationToken);
}
