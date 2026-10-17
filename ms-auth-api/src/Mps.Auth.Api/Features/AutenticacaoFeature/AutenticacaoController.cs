using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mps.Auth.Api.Extensions.Https;
using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.HolosCore.Api.Extensions.Https;

namespace Mps.Auth.Api.Features.AutenticacaoFeature;

[Route("autenticacao")]
[ApiController]
public class AutenticacaoController
(
    IMediator mediator,
    IConfiguration configuration
) : ControllerBase
{
    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    [HttpPost("autenticar")]
    [AllowAnonymous]
    public async Task<ActionResult> Post
    (
        [FromBody] AutenticarUsuarioExternalCommand request,
        CancellationToken cancellationToken
    )
    {
        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPost("autenticar-com-ad")]
    public async Task<ActionResult> Post
    (
        [FromBody] AutenticarUsuarioComAdCommand request,
        CancellationToken cancellationToken
    )
    {
        request.IdToken = HttpContext.TryGetJwtToken();
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPost("autenticar-com-gov")]
    public async Task<ActionResult> Post
    (
        [FromQuery] AutenticarUsuarioComGovCommand request,
        CancellationToken cancellationToken
    )
    {
        request.IdToken = HttpContext.TryGetJwtToken();

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPost("autenticar-com-gov-autorizacao-many")]
    [AllowAnonymous]
    public async Task<ActionResult> Post
    (
        [FromBody] AutenticarUsuarioComGovAutorizacaoManyCommand request,
        CancellationToken cancellationToken
    )
    {
        request.IdToken = HttpContext.TryGetJwtToken();

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPost("impersonate")]
    public async Task<ActionResult> Post
    (
        [FromBody] AutenticarImpersonateCommand request,
        CancellationToken cancellationToken
    )
    {
        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPost("deslogar")]
    [AllowAnonymous]
    public async Task<ActionResult> Post
    (
        [FromBody] DeslogarUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }
}