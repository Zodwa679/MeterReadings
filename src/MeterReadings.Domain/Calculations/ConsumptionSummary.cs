namespace MeterReadings.Domain.Calculations;

public record ConsumptionSummary(
    int ReadingCount,
    decimal TotalKwh,
    IReadOnlyList<DailyConsumption> Daily);

public record DailyConsumption(DateOnly DateUtc, decimal Kwh);
