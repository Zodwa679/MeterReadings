using MeterReadings.Api.Infrastructure;
using MeterReadings.Application.Summaries;
using Microsoft.AspNetCore.Mvc;

namespace MeterReadings.Api.Controllers;

[ApiController]
[Route("api/meters/{meterId:int}/summary")]
public sealed class SummaryController(ISummaryService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SummaryResponse>> Get(
        int meterId,
        [FromQuery] string? fromUtc,
        [FromQuery] string? toUtc,
        CancellationToken cancellationToken)
    {
        var from = QueryTimestamps.Parse(fromUtc, nameof(fromUtc), required: true, ModelState);
        var to = QueryTimestamps.Parse(toUtc, nameof(toUtc), required: true, ModelState);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (from > to)
        {
            ModelState.AddModelError("fromUtc", "'fromUtc' must be earlier than or equal to 'toUtc'.");
            return ValidationProblem(ModelState);
        }

        return Ok(await service.GetAsync(meterId, new SummaryQuery(from!.Value, to!.Value), cancellationToken));
    }
}
