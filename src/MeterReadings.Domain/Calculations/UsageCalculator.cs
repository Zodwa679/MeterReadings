using MeterReadings.Domain.Entities;

namespace MeterReadings.Domain.Calculations;

public sealed class UsageCalculator
{
    public static decimal Calculate(MeterReading previous, MeterReading current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        if (current.IsReset)
        {
            return current.CumulativeKwh;
        }

        return current.CumulativeKwh >= previous.CumulativeKwh
            ? current.CumulativeKwh - previous.CumulativeKwh
            : 0m;
    }
}
