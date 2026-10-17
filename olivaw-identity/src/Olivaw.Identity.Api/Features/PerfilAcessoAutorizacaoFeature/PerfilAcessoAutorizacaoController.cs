using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;
using Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;

namespace Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature;

[ApiController]
[Route("identity/perfis-autorizacoes")]
[Authorize(Policy = "identity.gerenciar")]
public sealed class PerfilAcessoAutorizacaoController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<InserirPerfilAcessoAutorizacaoResponse>> Inserir(InserirPerfilAcessoAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AtualizarPerfilAcessoAutorizacaoResponse>> Atualizar(Guid id, AtualizarPerfilAcessoAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RemoverPerfilAcessoAutorizacaoResponse>> Remover(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RemoverPerfilAcessoAutorizacaoCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SelecionarPerfilAcessoAutorizacaoByIdResponse>> Selecionar(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new SelecionarPerfilAcessoAutorizacaoByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SelecionarPerfilAcessoAutorizacaoByFilterResponse>> Selecionar([FromQuery] SelecionarPerfilAcessoAutorizacaoByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
