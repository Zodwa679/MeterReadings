using MeterReadings.Application.Common;
using MeterReadings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeterReadings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Set it in appsettings.Development.json " +
                "or via the ConnectionStrings__DefaultConnection environment variable.");

        services.AddDbContext<MeterReadingsDbContext>(options => options
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<MeterReadingsDbContext>());

        return services;
    }
}
