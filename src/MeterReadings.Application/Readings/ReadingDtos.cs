using System.ComponentModel.DataAnnotations;

namespace MeterReadings.Application.Readings;

public record CreateReadingRequest(
    [Required] DateTime? TimestampUtc,
    [Required][Range(0.0, double.MaxValue)] decimal? CumulativeKwh,
    bool IsReset = false);

public record ReadingResponse(
    long Id,
    int MeterId,
    DateTime TimestampUtc,
    decimal CumulativeKwh,
    bool IsReset,
    DateTime CreatedAt);
