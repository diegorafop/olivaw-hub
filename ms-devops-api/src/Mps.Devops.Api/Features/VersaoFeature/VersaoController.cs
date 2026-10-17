using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mps.Devops.Api.Features.VersaoFeature.Queries;

namespace Mps.Devops.Api.Features.VersaoFeature;

[Route("version")]
[ApiController]
public class VersaoController
(
    IMediator mediator
) : ControllerBase
{
    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    [HttpGet]
    public async Task<IActionResult> Get
    (
        [FromQuery] SelecionarVersaoQuery request,
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
}