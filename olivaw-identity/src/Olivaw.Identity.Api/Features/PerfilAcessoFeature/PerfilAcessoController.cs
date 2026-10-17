using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.PerfilAcessoFeature.Commands;
using Olivaw.Identity.Api.Features.PerfilAcessoFeature.Queries;

namespace Olivaw.Identity.Api.Features.PerfilAcessoFeature;

[ApiController]
[Route("identity/perfis-acesso")]
[Authorize(Policy = "identity.gerenciar")]
public sealed class PerfilAcessoController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<InserirPerfilAcessoResponse>> Inserir(InserirPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AtualizarPerfilAcessoResponse>> Atualizar(Guid id, AtualizarPerfilAcessoCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RemoverPerfilAcessoResponse>> Remover(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RemoverPerfilAcessoCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SelecionarPerfilAcessoByIdResponse>> Selecionar(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new SelecionarPerfilAcessoByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SelecionarPerfilAcessoByFilterResponse>> Selecionar([FromQuery] SelecionarPerfilAcessoByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
