using MediatR;
using Microsoft.AspNetCore.Mvc;
using Owh.Devops.Api.Features.VersionFeature.Queries;

namespace Owh.Devops.Api.Features.VersionFeature;

[Route("version")]
[ApiController]
public class VersionController
(
    IMediator mediator
) : ControllerBase
{
    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    [HttpGet]
    public async Task<IActionResult> Get
    (
        [FromQuery] ReadVersionQuery request,
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