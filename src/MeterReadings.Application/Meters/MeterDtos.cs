using System.ComponentModel.DataAnnotations;
using MeterReadings.Domain.Enums;

namespace MeterReadings.Application.Meters;

public record CreateMeterRequest(
    [Required][MaxLength(50)] string? MeterNumber,
    MeterType? MeterType,
    MeterStatus? Status);

public record MeterResponse(
    int Id,
    int SiteId,
    string MeterNumber,
    MeterType MeterType,
    MeterStatus Status,
    DateTime CreatedAt);
