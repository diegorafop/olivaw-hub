using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.PortalFeature.Commands;
using Owh.Olivaw.Api.Features.PortalFeature.Queries;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
namespace Owh.Olivaw.Api.Features.PortalFeature;
[ApiController,Route("portal/home"),Authorize(Policy="portal.member"),ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class PortalHomeController(IMediator mediator):ControllerBase
{
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadPortalHomeQuery query,CancellationToken token)=>Ok(await mediator.Send(new ReadPortalHomeMediatorQuery{Payload=query,ActorUserId=Guid.Parse(User.FindFirstValue("sub")!)},token));
}
[ApiController,Route("portal-preview/home"),Authorize(Policy="olivaw.root"),ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class PortalHomePreviewController(IMediator mediator,CurrentActor actor):ControllerBase
{
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadPortalHomeQuery query,CancellationToken token)=>Ok(await mediator.Send(new ReadPortalHomeMediatorQuery{Payload=query,ActorUserId=actor.Id,Preview=true},token));
}
[ApiController,Route("portal-usage-samples"),Authorize(Policy="olivaw.root"),ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class PortalUsageSampleController(IMediator mediator,CurrentActor actor):ControllerBase
{
    [HttpPost]public async Task<IActionResult>Create([FromBody]CreatePortalUsageSampleCommand command,CancellationToken token){var result=await mediator.Send(new CreatePortalUsageSampleMediatorCommand{Payload=command,ActorUserId=actor.Id},token);if(result.Conflict!=null)return Conflict(new{detail=result.Conflict});return StatusCode(201,result.Response);}
}
