namespace MeterReadings.Application.Summaries;

public interface ISummaryService
{
    Task<SummaryResponse> GetAsync(int meterId, SummaryQuery query, CancellationToken cancellationToken);
}
