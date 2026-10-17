using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mps.Auth.Api.Features.PerfilAcessoFeature.Commands;
using Mps.Auth.Api.Features.PerfilAcessoFeature.Queries;
using Mps.HolosCore.Api.Extensions.Https;

namespace Mps.Auth.Api.Features.PerfisAcessoFeature;

[Route("perfil-acesso")]
[ApiController]
public class PerfilAcessoController
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
        [FromRoute] SelecionarPerfilAcessoByIdQuery request,
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
        [FromQuery] SelecionarPerfilAcessoByFilterQuery request,
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
        [FromBody] InserirPerfilAcessoCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);
        return Ok(await _mediator.Send(request));
    }

    [HttpPut]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarPerfilAcessoCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);
        return Ok(await _mediator.Send(request));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var request = new RemoverPerfilAcessoCommand { Id = id };
        return Ok(await _mediator.Send(request));
    }

    [HttpPut("alternar")]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Alternar
    (
        [FromBody] AlternarPerfilAcessoCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);
        return Ok(await _mediator.Send(request));
    }
}