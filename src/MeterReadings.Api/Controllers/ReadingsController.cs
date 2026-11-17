using MeterReadings.Api.Infrastructure;
using MeterReadings.Application.Readings;
using Microsoft.AspNetCore.Mvc;

namespace MeterReadings.Api.Controllers;

[ApiController]
[Route("api/meters/{meterId:int}/readings")]
public sealed class ReadingsController(IReadingService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReadingResponse>> Create(
        int meterId, CreateReadingRequest request, CancellationToken cancellationToken)
    {
        var reading = await service.AddAsync(meterId, request, cancellationToken);

        return CreatedAtAction(nameof(List), new { meterId }, reading);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ReadingResponse>>> List(
        int meterId,
        [FromQuery] string? fromUtc,
        [FromQuery] string? toUtc,
        CancellationToken cancellationToken)
    {
        var from = QueryTimestamps.Parse(fromUtc, nameof(fromUtc), required: false, ModelState);
        var to = QueryTimestamps.Parse(toUtc, nameof(toUtc), required: false, ModelState);

        if (ModelState.IsValid && from > to)
        {
            ModelState.AddModelError(nameof(fromUtc), "'fromUtc' must be earlier than or equal to 'toUtc'.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        return Ok(await service.GetAsync(meterId, from, to, cancellationToken));
    }
}
