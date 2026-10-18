using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.AccessProfileFeature.Commands;
using Olivaw.Identity.Api.Features.AccessProfileFeature.Queries;

namespace Olivaw.Identity.Api.Features.AccessProfileFeature;

[ApiController]
[Route("identity/accessprofiles")]
[Authorize(Policy = "identity.manage")]
public sealed class AccessProfileController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateAccessProfileResponse>> Create(CreateAccessProfileCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateAccessProfileResponse>> Update(Guid id, UpdateAccessProfileCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<DeleteAccessProfileResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteAccessProfileCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReadAccessProfileByIdResponse>> Read(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadAccessProfileByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<ReadAccessProfileByFilterResponse>> Read([FromQuery] ReadAccessProfileByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
