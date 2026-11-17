namespace MeterReadings.Application.Meters;

public interface IMeterService
{
    Task<IReadOnlyList<MeterResponse>> GetBySiteAsync(int siteId, CancellationToken cancellationToken);
    Task<MeterResponse> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<MeterResponse> CreateAsync(int siteId, CreateMeterRequest request, CancellationToken cancellationToken);
}
