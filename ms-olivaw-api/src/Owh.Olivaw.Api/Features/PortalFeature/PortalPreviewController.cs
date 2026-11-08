using Owh.Olivaw.Api.Features.DeliveryNoteFeature.Queries;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.PortalFeature.Queries;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
namespace Owh.Olivaw.Api.Features.PortalFeature;
[ApiController, Route("portal-preview"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class PortalPreviewController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet("requests")] public async Task<IActionResult> ReadRequests([FromQuery] ReadPortalRequestsQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalRequestsMediatorQuery { Payload = query, ActorUserId = actor.Id, Preview = true }, token));
    [HttpGet("requests/messages")] public async Task<IActionResult> ReadMessages([FromQuery] ReadPortalMessagesQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalMessagesMediatorQuery { Payload = query, ActorUserId = actor.Id, Preview = true }, token));
    [HttpGet("deliveries")] public async Task<IActionResult> ReadDeliveries([FromQuery] ReadPortalDeliveriesQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalDeliveriesMediatorQuery { Payload = query, ActorUserId = actor.Id, Preview = true }, token));
    [HttpGet("dashboard")] public async Task<IActionResult> ReadDashboard([FromQuery] ReadPortalDashboardQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalDashboardMediatorQuery { Payload = query, ActorUserId = actor.Id, Preview = true }, token));
}
