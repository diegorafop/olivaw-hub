using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;
using Mps.HolosCore.Api.Extensions.Https;

namespace Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature;

[Route("perfil-acesso-autorizacao")]
[ApiController]
public class PerfilAcessoAutorizacaoController
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
        [FromQuery] SelecionarPerfilAcessoAutorizacaoByFilterQuery request
    )
    {
        return Ok(await _mediator.Send(request));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Get
    (
        [FromRoute] SelecionarPerfilAcessoAutorizacaoByIdQuery request,
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
        [FromBody] InserirPerfilAcessoAutorizacaoCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);
        return Ok(await _mediator.Send(request));
    }

    [HttpPut]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarPerfilAcessoAutorizacaoCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);
        return Ok(await _mediator.Send(request));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "root, PerfilAcesso")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var request = new RemoverPerfilAcessoAutorizacaoCommand { Id = id };
        return Ok(await _mediator.Send(request));
    }
}