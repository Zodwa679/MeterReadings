using MeterReadings.Application.Meters;
using MeterReadings.Application.Readings;
using MeterReadings.Application.Sites;
using MeterReadings.Application.Summaries;
using MeterReadings.Domain.Calculations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MeterReadings.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<ConsumptionSummaryCalculator>();

        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<IMeterService, MeterService>();
        services.AddScoped<IReadingService, ReadingService>();
        services.AddScoped<ISummaryService, SummaryService>();

        return services;
    }
}
