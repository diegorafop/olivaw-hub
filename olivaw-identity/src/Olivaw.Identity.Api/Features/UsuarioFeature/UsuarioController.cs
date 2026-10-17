using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.UsuarioFeature.Commands;
using Olivaw.Identity.Api.Features.UsuarioFeature.Queries;

namespace Olivaw.Identity.Api.Features.UsuarioFeature;

[ApiController]
[Route("identity/usuarios")]
[Authorize(Policy = "identity.gerenciar")]
public sealed class UsuarioController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<InserirUsuarioResponse>> Inserir(InserirUsuarioCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AtualizarUsuarioResponse>> Atualizar(Guid id, AtualizarUsuarioCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RemoverUsuarioResponse>> Remover(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RemoverUsuarioCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SelecionarUsuarioByIdResponse>> Selecionar(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new SelecionarUsuarioByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SelecionarUsuarioByFilterResponse>> Selecionar([FromQuery] SelecionarUsuarioByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
