using MeterReadings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Application.Common;

public interface IApplicationDbContext
{
    DbSet<Site> Sites { get; }
    DbSet<Meter> Meters { get; }
    DbSet<MeterReading> MeterReadings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
