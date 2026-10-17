using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mps.Auth.Api.Features.ReadyFeature.Queries;

namespace Mps.Auth.Api.Features.ReadyFeature;

[ApiController]
[Route("ready")]
public class ReadyController
(
    IMediator mediator
) : ControllerBase
{
    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    [HttpGet]
    public async Task<IActionResult> Get
    (
        [FromQuery] SelecionarReadyQuery request
    )
    {
        return Ok(await _mediator.Send(request));
    }
}