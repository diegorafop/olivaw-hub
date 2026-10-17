using MediatR;
using Olivaw.Identity.Api.Features.AutenticacaoFeature.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Olivaw.Identity.Api.Features.AutenticacaoFeature.Commands;

namespace Olivaw.Identity.Api.Features.AutenticacaoFeature;

[ApiController]
[Route("identity/autenticacao")]
[Authorize(AuthenticationSchemes = "OlivawIdentity")]
public sealed class AutenticacaoController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("identity-login")]
    public async Task<ActionResult<AutenticarUsuarioResponse>> Login(AutenticarUsuarioCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<DeslogarUsuarioResponse>> Logout(DeslogarUsuarioCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("senha")]
    public async Task<ActionResult<AlterarSenhaResponse>> AlterarSenha(AlterarSenhaCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
    [HttpGet("acesso")]
    public async Task<ActionResult<SelecionarAcessoAtualResponse>> SelecionarAcesso(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new SelecionarAcessoAtualQuery(), cancellationToken));
    }
}
