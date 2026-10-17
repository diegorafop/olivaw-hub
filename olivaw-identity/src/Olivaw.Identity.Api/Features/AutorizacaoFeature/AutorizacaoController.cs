using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.AutorizacaoFeature.Commands;
using Olivaw.Identity.Api.Features.AutorizacaoFeature.Queries;

namespace Olivaw.Identity.Api.Features.AutorizacaoFeature;

[ApiController]
[Route("identity/autorizacoes")]
[Authorize(Policy = "identity.gerenciar")]
public sealed class AutorizacaoController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<InserirAutorizacaoResponse>> Inserir(InserirAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AtualizarAutorizacaoResponse>> Atualizar(Guid id, AtualizarAutorizacaoCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RemoverAutorizacaoResponse>> Remover(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RemoverAutorizacaoCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SelecionarAutorizacaoByIdResponse>> Selecionar(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new SelecionarAutorizacaoByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<SelecionarAutorizacaoByFilterResponse>> Selecionar([FromQuery] SelecionarAutorizacaoByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
