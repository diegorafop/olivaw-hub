using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature.Commands;
using Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature.Queries;

namespace Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature;

[ApiController]
[Route("identity/usuarios-perfis")]
[Authorize(Policy = "identity.gerenciar")]
public sealed class UsuarioPerfilAcessoController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<InserirUsuarioPerfilAcessoResponse>> Inserir(InserirUsuarioPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AtualizarUsuarioPerfilAcessoResponse>> Atualizar(Guid id, AtualizarUsuarioPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RemoverUsuarioPerfilAcessoResponse>> Remover(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RemoverUsuarioPerfilAcessoCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SelecionarUsuarioPerfilAcessoByIdResponse>> Selecionar(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new SelecionarUsuarioPerfilAcessoByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SelecionarUsuarioPerfilAcessoByFilterResponse>> Selecionar([FromQuery] SelecionarUsuarioPerfilAcessoByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
