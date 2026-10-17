using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mps.Auth.Api.Features.VersaoFeature.Queries;

namespace Mps.Auth.Api.Features.VersaoFeature;

[Route("version")]
[ApiController]
public class VersaoController
(
    IMediator mediator
) : ControllerBase
{
    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get
    (
        [FromQuery] SelecionarVersaoExternalQuery request,
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