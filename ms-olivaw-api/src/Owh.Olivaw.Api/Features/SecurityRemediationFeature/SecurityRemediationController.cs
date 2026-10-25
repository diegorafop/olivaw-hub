using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.SecurityRemediationFeature.Commands;
using Owh.Olivaw.Api.Features.SecurityRemediationFeature.Queries;
using Owh.Olivaw.Client.Features.SecurityRemediationFeature.Commands;
using Owh.Olivaw.Client.Features.SecurityRemediationFeature.Queries;
namespace Owh.Olivaw.Api.Features.SecurityRemediationFeature;
[ApiController, Route("security-remediations")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class SecurityRemediationController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet, Authorize(Policy = "olivaw.root")]
    public async Task<IActionResult> Read([FromQuery] ReadSecurityRemediationQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadSecurityRemediationMediatorQuery { Payload = query }, token));
    [HttpPost, Authorize(Policy = "olivaw.root"), EnableRateLimiting("security-report-write")]
    public async Task<IActionResult> Create([FromBody] CreateSecurityRemediationCommand command, CancellationToken token) => Ok(await mediator.Send(new CreateSecurityRemediationMediatorCommand { Payload = command, ActorUserId = actor.Id }, token));
    [HttpPost("batch"), Authorize(Policy = "olivaw.root"), EnableRateLimiting("security-report-write")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateSecurityRemediationBatchCommand command, CancellationToken token) => Ok(await mediator.Send(new CreateSecurityRemediationBatchMediatorCommand { Payload = command, ActorUserId = actor.Id }, token));
    [HttpPut("{externalId:guid}/review"), Authorize(Policy = "olivaw.root")]
    public async Task<IActionResult> Review(Guid externalId, [FromBody] UpdateSecurityRemediationReviewCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new UpdateSecurityRemediationReviewMediatorCommand { ExternalId = externalId, ActorUserId = actor.Id, Payload = command }, token);
        if (result.Conflict) return Conflict(new { detail = "Revisão exige intervenção, prazo expirado e confirmação da parada." });
        return Ok(result);
    }
    [HttpGet("queue"), Authorize(Policy = "security.remediations.execute")]
    public async Task<IActionResult> ReadQueue([FromQuery] string hostId, CancellationToken token) => Ok(await mediator.Send(new ReadSecurityRemediationQueueMediatorQuery { HostId = hostId, ActorUserId = actor.Id }, token));
    [HttpPut("{externalId:guid}"), Authorize(Policy = "security.remediations.execute")]
    [RequestSizeLimit(131072)]
    public async Task<IActionResult> Update(Guid externalId, [FromBody] UpdateSecurityRemediationCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new UpdateSecurityRemediationMediatorCommand { ExternalId = externalId, Payload = command, ActorUserId = actor.Id }, token);
        if (result.Conflict) return Conflict(new { detail = "Autorização, estado ou prazo não permite esta transição." });
        return Ok(result);
    }
}
