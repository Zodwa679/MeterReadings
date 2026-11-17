namespace MeterReadings.Application.Readings;

public interface IReadingService
{
    Task<ReadingResponse> AddAsync(int meterId, CreateReadingRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<ReadingResponse>> GetAsync(int meterId, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken);
}
