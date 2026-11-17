using MeterReadings.Application.Meters;
using Microsoft.AspNetCore.Mvc;

namespace MeterReadings.Api.Controllers;

[ApiController]
public sealed class MetersController(IMeterService service) : ControllerBase
{
    [HttpGet("api/sites/{siteId:int}/meters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MeterResponse>>> GetBySite(int siteId, CancellationToken cancellationToken) =>
        Ok(await service.GetBySiteAsync(siteId, cancellationToken));

    [HttpGet("api/meters/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeterResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost("api/sites/{siteId:int}/meters")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MeterResponse>> Create(
        int siteId, CreateMeterRequest request, CancellationToken cancellationToken)
    {
        var meter = await service.CreateAsync(siteId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = meter.Id }, meter);
    }
}
