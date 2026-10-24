using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Owh.Olivaw.Api.Features.ReadyFeature.Queries;

namespace Owh.Olivaw.Api.Features.ReadyFeature;

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
        [FromQuery] ReadReadyQuery request,
        CancellationToken cancellationToken
    )
    {
        return Ok(await _mediator.Send(request, cancellationToken));
    }
}