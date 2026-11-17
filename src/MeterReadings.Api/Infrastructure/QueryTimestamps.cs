using MeterReadings.Application.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MeterReadings.Api.Infrastructure;

public static class QueryTimestamps
{
    public static DateTime? Parse(string? value, string parameterName, bool required, ModelStateDictionary modelState)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                modelState.AddModelError(parameterName, $"'{parameterName}' is required.");
            }

            return null;
        }

        if (!UtcDateTimeParser.TryParse(value, out var utc))
        {
            modelState.AddModelError(
                parameterName,
                $"'{parameterName}' must be an ISO 8601 timestamp ending in 'Z' or an explicit offset, e.g. 2026-11-01T00:00:00Z.");
            return null;
        }

        return utc;
    }
}
