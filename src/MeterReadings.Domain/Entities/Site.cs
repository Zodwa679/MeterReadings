using MeterReadings.Domain.Enums;

namespace MeterReadings.Domain.Entities;

public class Site
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public SiteType SiteType { get; set; }
    public SiteStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Meter> Meters { get; set; } = new List<Meter>();
}
