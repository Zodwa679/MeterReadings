using MeterReadings.Domain.Entities;
using MeterReadings.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeterReadings.Infrastructure.Persistence.Configurations;

public sealed class MeterConfiguration : IEntityTypeConfiguration<Meter>
{
    public void Configure(EntityTypeBuilder<Meter> builder)
    {
        builder.ToTable("Meters");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).UseIdentityColumn();

        builder.Property(m => m.MeterNumber).IsRequired().HasMaxLength(50);
        builder.Property(m => m.MeterType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.CreatedAt).IsRequired();

        builder.HasIndex(m => m.MeterNumber)
            .IsUnique()
            .HasDatabaseName("IX_Meters_MeterNumber");

        builder.HasOne(m => m.Site)
            .WithMany(s => s.Meters)
            .HasForeignKey(m => m.SiteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.Meters);
    }
}
