using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Owh.Olivaw.Api.Features.PortalFeature.Commands;
using Owh.Olivaw.Api.Features.PortalFeature.Queries;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
namespace Owh.Olivaw.Api.Features.PortalFeature;
[ApiController, Route("portal/requests"), Authorize(Policy = "portal.member")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class PortalRequestsController(IMediator mediator) : ControllerBase
{
    private Guid Actor => Guid.Parse(User.FindFirstValue("sub")!);
    private string Name() { var value = User.FindFirstValue("name") ?? "Cliente"; if (value.Length > 120) return value[..120]; return value; }
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadPortalRequestsQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalRequestsMediatorQuery { Payload = query, ActorUserId = Actor }, token));
    [HttpGet("messages")] public async Task<IActionResult> ReadMessages([FromQuery] ReadPortalMessagesQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalMessagesMediatorQuery { Payload = query, ActorUserId = Actor }, token));
    [HttpPost, EnableRateLimiting("lead-write")] public async Task<IActionResult> Create([FromBody] CreatePortalRequestCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreatePortalRequestMediatorCommand { Payload = command, ActorUserId = Actor, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict }); return StatusCode(201, result.Response);
    }
    [HttpPost("messages"), EnableRateLimiting("lead-write")] public async Task<IActionResult> CreateMessage([FromBody] CreatePortalMessageCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreatePortalMessageMediatorCommand { Payload = command, ActorUserId = Actor, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict }); return StatusCode(201, result.Response);
    }
}
