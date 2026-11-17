using MeterReadings.Domain.Entities;

namespace MeterReadings.Domain.Calculations;

public sealed class ConsumptionSummaryCalculator
{
    private const int KwhDecimals = 3;

    public ConsumptionSummary Calculate(
        IEnumerable<MeterReading> readings,
        DateTime fromUtc,
        DateTime toUtc)
    {
        ArgumentNullException.ThrowIfNull(readings);

        var inRange = readings
            .Where(r => r.TimestampUtc >= fromUtc && r.TimestampUtc <= toUtc)
            .OrderBy(r => r.TimestampUtc)
            .ToList();

        if (inRange.Count < 2)
        {
            return new ConsumptionSummary(inRange.Count, 0m, []);
        }

        decimal correctedTotal = 0m;
        var hasResetOrDecrease = false;
        var daily = new SortedDictionary<DateOnly, decimal>();

        for (var i = 1; i < inRange.Count; i++)
        {
            var previous = inRange[i - 1];
            var current = inRange[i];
            var kwh = UsageCalculator.Calculate(previous, current);

            correctedTotal += kwh;
            hasResetOrDecrease |= current.IsReset || current.CumulativeKwh < previous.CumulativeKwh;

            var date = DateOnly.FromDateTime(current.TimestampUtc);
            daily.TryGetValue(date, out var sum);
            daily[date] = kwh > 0m ? sum + kwh : sum;
        }

        // The brief's rule is latest minus earliest. After a reset or decrease (optional handling, see SOLUTION.md)
        // that would undercount or go negative, so the reset-aware sum of pair differences is used instead.
        var total = hasResetOrDecrease
            ? correctedTotal
            : inRange[^1].CumulativeKwh - inRange[0].CumulativeKwh;

        return new ConsumptionSummary(
            inRange.Count,
            Round(total),
            daily.Select(d => new DailyConsumption(d.Key, Round(d.Value))).ToList());
    }

    private static decimal Round(decimal value) =>
        Math.Round(value, KwhDecimals, MidpointRounding.AwayFromZero);
}
