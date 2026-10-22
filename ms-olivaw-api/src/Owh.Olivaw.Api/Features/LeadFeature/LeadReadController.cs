using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Owh.Olivaw.Api.Features.LeadFeature.Queries;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;

namespace Owh.Olivaw.Api.Features.LeadFeature;

[ApiController]
[Route("leads")]
[Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class LeadReadController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ReadByFilter([FromQuery] ReadLeadByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadLeadByFilterMediatorQuery { Payload = request }, cancellationToken));
    }
    [HttpGet("{externalId:guid}")]
    public async Task<IActionResult> ReadById(Guid externalId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadLeadByIdMediatorQuery { Payload = new ReadLeadByIdQuery { ExternalId = externalId } }, cancellationToken));
    }
    [HttpGet("summary")]
    public async Task<IActionResult> ReadSummary(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadLeadSummaryMediatorQuery(), cancellationToken));
    }
}
