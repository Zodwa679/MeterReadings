using MeterReadings.Application.Sites;
using Microsoft.AspNetCore.Mvc;

namespace MeterReadings.Api.Controllers;

[ApiController]
[Route("api/sites")]
public sealed class SitesController(ISiteService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SiteResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SiteResponse>> Create(CreateSiteRequest request, CancellationToken cancellationToken)
    {
        var site = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = site.Id }, site);
    }
}
