using MeterReadings.Application.Common;
using MeterReadings.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Infrastructure.Persistence;

public sealed class MeterReadingsDbContext(DbContextOptions<MeterReadingsDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Meter> Meters => Set<Meter>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeterReadingsDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeValueConverter>()
            .HaveColumnType("datetime2");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException
        {
            Number: UniqueIndexViolation or UniqueConstraintViolation
        })
        {
            throw new ConflictException("A record with the same unique value already exists.", ex);
        }
    }
}
