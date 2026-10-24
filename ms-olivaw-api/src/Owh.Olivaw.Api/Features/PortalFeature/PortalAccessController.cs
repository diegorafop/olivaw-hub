using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.PortalFeature.Commands;
using Owh.Olivaw.Api.Features.PortalFeature.Queries;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
namespace Owh.Olivaw.Api.Features.PortalFeature;
[ApiController, Route("portal-access"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class PortalAccessController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpPost("services")] public async Task<IActionResult> CreateService([FromBody] CreatePortalServiceLinkCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreatePortalServiceLinkMediatorCommand { Payload = command, ActorUserId = actor.Id }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict }); return StatusCode(201, result.Response);
    }
    [HttpGet("audit")] public async Task<IActionResult> ReadAudit([FromQuery] ReadPortalAccessAuditQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalAccessAuditMediatorQuery { Payload = query }, token));
    [HttpPost("{id:guid}/recovery")] public async Task<IActionResult> Recover(Guid id, [FromBody] UpdatePortalRecoveryCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new UpdatePortalRecoveryMediatorCommand { ExternalId = id, Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict }); return Ok(result.Response);
    }
    [HttpGet("services")] public async Task<IActionResult> ReadServices([FromQuery] ReadPortalServiceConfigurationQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalServiceConfigurationMediatorQuery { Payload = query }, token));
    [HttpPut("services/{id:guid}")] public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdatePortalServiceLinkCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new UpdatePortalServiceLinkMediatorCommand { ExternalId = id, Payload = command, ActorUserId = actor.Id }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict }); return Ok(result.Response);
    }
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadPortalMembershipByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalMembershipByFilterMediatorQuery { Payload = query }, token));
    [HttpPost("invitations")] public async Task<IActionResult> CreateInvitation([FromBody] CreatePortalInvitationCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreatePortalInvitationMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict }); return StatusCode(201, result.Response);
    }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Revoke(Guid id, [FromBody] UpdatePortalMembershipCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new UpdatePortalMembershipMediatorCommand { ExternalId = id, Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict }); return Ok(result.Response);
    }
    private string Name() { var name = User.FindFirst("name")?.Value ?? "Operação Olivaw"; if (name.Length > 120) return name[..120]; return name; }
}
