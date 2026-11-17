using MeterReadings.Domain.Enums;

namespace MeterReadings.Domain.Entities;

public class Meter
{
    public int Id { get; set; }
    public string MeterNumber { get; set; } = string.Empty;
    public MeterType MeterType { get; set; }
    public MeterStatus Status { get; set; }

    public int SiteId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Site? Site { get; set; }
    public ICollection<MeterReading> Readings { get; set; } = new List<MeterReading>();
}
