namespace MeterReadings.Application.Summaries;

public record SummaryQuery(DateTime FromUtc, DateTime ToUtc);

public record SummaryResponse(
    int MeterId,
    DateTime FromUtc,
    DateTime ToUtc,
    int ReadingCount,
    decimal TotalKwh,
    IReadOnlyList<DailyConsumptionResponse> Daily);

public record DailyConsumptionResponse(DateOnly DateUtc, decimal Kwh);
