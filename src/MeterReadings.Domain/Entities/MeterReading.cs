namespace MeterReadings.Domain.Entities;

public class MeterReading
{
    public long Id { get; set; }
    public int MeterId { get; set; }

    public DateTime TimestampUtc { get; set; }

    public decimal CumulativeKwh { get; set; }

    public bool IsReset { get; set; }

    public DateTime CreatedAt { get; set; }

    public Meter? Meter { get; set; }
}
