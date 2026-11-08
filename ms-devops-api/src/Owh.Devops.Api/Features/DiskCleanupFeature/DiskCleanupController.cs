using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Owh.Devops.Api.Features.DiskCleanupFeature.Commands;
using Owh.Devops.Api.Features.DiskCleanupFeature.Queries;

namespace Owh.Devops.Api.Features.DiskCleanupFeature;

[ApiController]
[Route("disk-cleanups")]
[Authorize(Policy = "devops.root")]
public sealed class DiskCleanupController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateDiskCleanupResponse>> Create(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateDiskCleanupCommand(), cancellationToken);
        return AcceptedAtAction(nameof(Read), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReadDiskCleanupResponse>> Read(Guid id, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(await mediator.Send(new ReadDiskCleanupQuery { Id = id }, cancellationToken));
    }
}
