using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mps.Limpo.Api.Features.ReadyFeature.Queries;

namespace Mps.Limpo.Api.Features.ReadyFeature;

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
    [AllowAnonymous]
    public async Task<IActionResult> Get
    (
        [FromQuery] SelecionarReadyQuery request
    )
    {
        return Ok(await _mediator.Send(request));
    }
}