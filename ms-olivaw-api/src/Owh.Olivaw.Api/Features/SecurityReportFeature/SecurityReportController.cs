using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.SecurityReportFeature.Commands;
using Owh.Olivaw.Api.Features.SecurityReportFeature.Queries;
using Owh.Olivaw.Client.Features.SecurityReportFeature.Commands;
using Owh.Olivaw.Client.Features.SecurityReportFeature.Queries;

namespace Owh.Olivaw.Api.Features.SecurityReportFeature;

[ApiController, Route("security-reports")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class SecurityReportController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpPost, Authorize(Policy = "security.reports.write"), EnableRateLimiting("security-report-write")]
    [RequestSizeLimit(1048576)]
    public async Task<IActionResult> Create([FromBody] CreateSecurityReportCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreateSecurityReportMediatorCommand { Payload = command, ActorUserId = actor.Id }, token);
        if (result.AlreadyReceived) return Ok(result);
        return StatusCode(201, result);
    }

    [HttpGet, Authorize(Policy = "olivaw.root")]
    public async Task<IActionResult> Read([FromQuery] ReadSecurityReportByFilterQuery query, CancellationToken token)
    {
        return Ok(await mediator.Send(new ReadSecurityReportByFilterMediatorQuery { Payload = query }, token));
    }
    [HttpGet("{externalId:guid}"), Authorize(Policy = "olivaw.root")]
    public async Task<IActionResult> ReadById(Guid externalId, CancellationToken token)
    {
        var result = await mediator.Send(new ReadSecurityReportByIdMediatorQuery { Payload = new() { ExternalId = externalId } }, token);
        if (result.Item == null) return NotFound();
        return Ok(result);
    }
}
