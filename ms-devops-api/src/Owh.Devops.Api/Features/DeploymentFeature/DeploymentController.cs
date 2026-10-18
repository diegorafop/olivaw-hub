using MediatR;
using Owh.Devops.Api.Features.DeploymentFeature.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Owh.Devops.Api.Features.DeploymentFeature.Commands;

namespace Owh.Devops.Api.Features.DeploymentFeature;

[ApiController]
[Route("deployments")]
[Authorize(Policy = "devops.deploy")]
public sealed class DeploymentController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpPost]
    public async Task<ActionResult<CreateDeploymentResponse>> Create([FromBody] CreateDeploymentCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return AcceptedAtAction(nameof(Read), new { id = response.Id }, response);
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReadDeploymentResponse>> Read([FromRoute] ReadDeploymentQuery request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}
