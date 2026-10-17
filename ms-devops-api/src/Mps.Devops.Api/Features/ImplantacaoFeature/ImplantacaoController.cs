using MediatR;
using Mps.Devops.Api.Features.ImplantacaoFeature.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mps.Devops.Api.Features.ImplantacaoFeature.Commands;

namespace Mps.Devops.Api.Features.ImplantacaoFeature;

[ApiController]
[Route("implantacoes")]
[Authorize(Policy = "devops.implantar")]
public sealed class ImplantacaoController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpPost]
    public async Task<ActionResult<CriarImplantacaoResponse>> Post([FromBody] CriarImplantacaoCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return AcceptedAtAction(nameof(Get), new { id = response.Id }, response);
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetDeploymentResponse>> Get([FromRoute] GetDeploymentQuery request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}
