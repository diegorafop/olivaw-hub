using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Owh.Olivaw.Api.Features.DeliveryNoteFeature.Queries;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
namespace Owh.Olivaw.Api.Features.DeliveryNoteFeature;
[ApiController, Route("portal/deliveries"), Authorize(Policy = "portal.member")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class PortalDeliveriesController(IMediator mediator) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadPortalDeliveriesQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalDeliveriesMediatorQuery { Payload = query, ActorUserId = Guid.Parse(User.FindFirstValue("sub")!) }, token));
}
