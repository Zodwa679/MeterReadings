using System.Globalization;
using System.Text.RegularExpressions;

namespace MeterReadings.Application.Common;

public static partial class UtcDateTimeParser
{
    [GeneratedRegex(@"(Z|[+-]\d{2}:\d{2})$")]
    private static partial Regex ZoneSuffix();

    public static bool TryParse(string? value, out DateTime utc)
    {
        utc = default;

        if (string.IsNullOrWhiteSpace(value) || !ZoneSuffix().IsMatch(value))
        {
            return false;
        }

        if (!DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out utc))
        {
            return false;
        }

        utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return true;
    }
}
