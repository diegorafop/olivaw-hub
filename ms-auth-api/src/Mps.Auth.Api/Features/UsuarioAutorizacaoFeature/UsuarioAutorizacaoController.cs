using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Commands;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Queries;
using Mps.HolosCore.Api.Extensions.Https;

namespace Mps.Auth.Api.Features.UsuarioAutorizacaoFeature;

[Route("usuario-autorizacao")]
[ApiController]
public class UsuarioAutorizacaoController
(
    IMediator mediator,
    IConfiguration configuration
) : ControllerBase
{
    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    [HttpGet("{id}")]
    public async Task<ActionResult> Get
    (
        [FromRoute] SelecionarUsuarioAutorizacaoByIdQuery request,
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

    [HttpGet]
    public async Task<ActionResult> Get
    (
        [FromQuery] SelecionarUsuarioAutorizacaoByFilterQuery request,
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

    [HttpPost]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Post
    (
        [FromBody] InserirUsuarioAutorizacaoCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);
        return Ok(await _mediator.Send(request));
    }

    [HttpPut]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarUsuarioAutorizacaoCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);
        return Ok(await _mediator.Send(request));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Delete
    (
        [FromRoute] RemoverUsuarioAutorizacaoCommand request,
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
}