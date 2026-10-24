using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Owh.Olivaw.Api.Features.PortalFeature.Commands;
using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Owh.Olivaw.Api.Features.PortalFeature.Queries;
using Owh.Olivaw.Client.Features.PortalFeature.Commands;
using Owh.Olivaw.Client.Features.PortalFeature.Queries;
namespace Owh.Olivaw.Api.Features.PortalFeature;
[ApiController, Route("portal"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class PortalSessionController(IMediator mediator) : ControllerBase
{
    [HttpPost("sessions"), AllowAnonymous, EnableRateLimiting("identity-login")]
    public async Task<IActionResult> CreateSession([FromBody] CreatePortalSessionCommand command, CancellationToken token) => Ok(await mediator.Send(new CreatePortalSessionMediatorCommand { Payload = command }, token));
    [HttpPost("invitations/read"), AllowAnonymous, EnableRateLimiting("identity-login")]
    public async Task<IActionResult> ReadInvitation([FromBody] ReadPortalInvitationQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPortalInvitationMediatorQuery { Payload = query }, token));
    [HttpPost("invitations/accept"), AllowAnonymous, EnableRateLimiting("identity-login")]
    public async Task<IActionResult> Accept([FromBody] UpdatePortalInvitationCommand command, CancellationToken token)
    {
        var authenticated = await HttpContext.AuthenticateAsync(PortalAuthority.Scheme);
        var user = Guid.Empty;
        if (authenticated.Succeeded) Guid.TryParse(authenticated.Principal?.FindFirstValue("sub"), out user);
        return Ok(await mediator.Send(new UpdatePortalInvitationMediatorCommand { Payload = command, AuthenticatedPortalUserId = user }, token));
    }
    [HttpDelete("sessions"), Authorize(Policy = "portal.member")]
    public async Task<IActionResult> DeleteSession(CancellationToken token) => Ok(await mediator.Send(new DeletePortalSessionMediatorCommand { ActorUserId = Guid.Parse(User.FindFirstValue("sub")!) }, token));
}
