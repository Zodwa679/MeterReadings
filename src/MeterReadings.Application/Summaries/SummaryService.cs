using MeterReadings.Application.Common;
using MeterReadings.Domain.Calculations;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Application.Summaries;

public sealed class SummaryService(IApplicationDbContext db, ConsumptionSummaryCalculator calculator) : ISummaryService
{
    public async Task<SummaryResponse> GetAsync(int meterId, SummaryQuery query, CancellationToken cancellationToken)
    {
        if (!await db.Meters.AnyAsync(m => m.Id == meterId, cancellationToken))
        {
            throw new NotFoundException($"Meter {meterId} was not found.");
        }

        var readings = await db.MeterReadings
            .AsNoTracking()
            .Where(r => r.MeterId == meterId && r.TimestampUtc >= query.FromUtc && r.TimestampUtc <= query.ToUtc)
            .ToListAsync(cancellationToken);

        var summary = calculator.Calculate(readings, query.FromUtc, query.ToUtc);

        return new SummaryResponse(
            meterId,
            query.FromUtc,
            query.ToUtc,
            summary.ReadingCount,
            summary.TotalKwh,
            summary.Daily.Select(d => new DailyConsumptionResponse(d.DateUtc, d.Kwh)).ToList());
    }
}
