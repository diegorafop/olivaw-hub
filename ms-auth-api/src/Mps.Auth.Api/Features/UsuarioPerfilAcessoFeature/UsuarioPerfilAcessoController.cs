using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Commands;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Queries;
using Mps.HolosCore.Api.Extensions.Https;

namespace Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature;

[Route("usuario-perfil-acesso")]
[ApiController]
public class UsuarioPerfilAcessoController
(
    IMediator mediator,
    IConfiguration configuration
) : ControllerBase
{
    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    [HttpGet]
    public async Task<ActionResult> Get
    (
        [FromQuery] SelecionarUsuarioPerfilAcessoByFilterQuery request,
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

    [HttpGet("{id}")]
    public async Task<ActionResult> Get
    (
        [FromRoute] SelecionarUsuarioPerfilAcessoByIdQuery request,
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
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Post
    (
        [FromBody] InserirUsuarioPerfilAcessoCommand request,
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

    [HttpPut]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarUsuarioPerfilAcessoCommand request,
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Delete
    (
        [FromRoute] RemoverUsuarioPerfilAcessoCommand request,
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