using MeterReadings.Domain.Entities;
using MeterReadings.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeterReadings.Infrastructure.Persistence.Configurations;

public sealed class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("Sites");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseIdentityColumn();

        builder.Property(s => s.Address).IsRequired().HasMaxLength(500);
        builder.Property(s => s.SiteType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasData(SeedData.Sites);
    }
}
