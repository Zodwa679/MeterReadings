using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using MeterReadings.Application.Common;

namespace MeterReadings.Api.Infrastructure;

public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Timestamps must be JSON strings in ISO 8601 format ending in 'Z' or an explicit offset.");
        }

        var value = reader.GetString();

        if (!UtcDateTimeParser.TryParse(value, out var utc))
        {
            throw new JsonException(
                $"'{value}' is not a valid UTC timestamp. Use ISO 8601 ending in 'Z' or an explicit offset, e.g. 2026-11-01T08:00:00Z.");
        }

        return utc;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        writer.WriteStringValue(utc.ToString(Format, CultureInfo.InvariantCulture));
    }
}
