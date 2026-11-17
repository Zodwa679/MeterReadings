using System.ComponentModel.DataAnnotations;
using MeterReadings.Domain.Enums;

namespace MeterReadings.Application.Sites;

public record CreateSiteRequest(
    [Required][MaxLength(500)] string? Address,
    SiteType? SiteType,
    SiteStatus? Status);

public record SiteResponse(int Id, string Address, SiteType SiteType, SiteStatus Status, DateTime CreatedAt);
