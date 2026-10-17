using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mps.Auth.Api.Features.AutorizacaoFeature.Commands;
using Mps.Auth.Api.Features.AutorizacaoFeature.Queries;
using Mps.HolosCore.Api.Extensions.Https;

namespace Mps.Auth.Api.Features.AutorizacaoFeature;

[Route("autorizacao")]
[ApiController]
public class AutorizacaoController
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
        [FromRoute] SelecionarAutorizacaoByIdQuery request,
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
        [FromQuery] SelecionarAutorizacaoByFilterQuery request
    )
    {
        return Ok(await _mediator.Send(request));
    }

    [HttpPost]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Post
    (
        [FromBody] InserirAutorizacaoCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);
        return Ok(await _mediator.Send(request));
    }

    [HttpPut]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarAutorizacaoCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);
        return Ok(await _mediator.Send(request));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Delete
    (
        [FromRoute] RemoverAutorizacaoCommand request,
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