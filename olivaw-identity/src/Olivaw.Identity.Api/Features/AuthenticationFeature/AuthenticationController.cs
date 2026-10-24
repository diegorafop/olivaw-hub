using MediatR;
using Olivaw.Identity.Api.Features.AuthenticationFeature.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Olivaw.Identity.Api.Features.AuthenticationFeature.Commands;

namespace Olivaw.Identity.Api.Features.AuthenticationFeature;

[ApiController]
[Route("identity/authentication")]
[Authorize(AuthenticationSchemes = "OlivawIdentity")]
public sealed class AuthenticationController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("identity-login")]
    public async Task<ActionResult<AuthenticateUserResponse>> Login(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<LogoutUserResponse>> Logout(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("password")]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePassword(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
    [HttpGet("access")]
    public async Task<ActionResult<ReadCurrentAccessResponse>> ReadAccess(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadCurrentAccessQuery(), cancellationToken));
    }
}
