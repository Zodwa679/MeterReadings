using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeterReadings.Infrastructure.Persistence;

public sealed class UtcDateTimeValueConverter() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : v,
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
