using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Owh.Olivaw.Api.Features.PortalFeature.Queries;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
namespace Owh.Olivaw.Api.Features.PortalFeature;
[ApiController, Route("portal"), Authorize(Policy = "portal.member")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class PortalDashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("companies")] public async Task<IActionResult> ReadCompanies([FromQuery] ReadPortalCompaniesQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalCompaniesMediatorQuery { Payload = query, ActorUserId = Guid.Parse(User.FindFirstValue("sub")!) }, token));
    [HttpGet("dashboard")] public async Task<IActionResult> ReadDashboard([FromQuery] ReadPortalDashboardQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalDashboardMediatorQuery { Payload = query, ActorUserId = Guid.Parse(User.FindFirstValue("sub")!) }, token));
}
