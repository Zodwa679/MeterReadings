using MeterReadings.Domain.Entities;
using MeterReadings.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeterReadings.Infrastructure.Persistence.Configurations;

public sealed class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> builder)
    {
        builder.ToTable("MeterReadings", t =>
            t.HasCheckConstraint("CK_MeterReadings_CumulativeKwh", "[CumulativeKwh] >= 0"));

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).UseIdentityColumn();

        builder.Property(r => r.TimestampUtc).IsRequired();
        builder.Property(r => r.CumulativeKwh).IsRequired().HasPrecision(12, 3);
        builder.Property(r => r.IsReset).IsRequired().HasDefaultValue(false);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => new { r.MeterId, r.TimestampUtc })
            .IsUnique()
            .HasDatabaseName("IX_MeterReadings_MeterId_TimestampUtc");

        builder.HasOne(r => r.Meter)
            .WithMany(m => m.Readings)
            .HasForeignKey(r => r.MeterId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.MeterReadings);
    }
}
