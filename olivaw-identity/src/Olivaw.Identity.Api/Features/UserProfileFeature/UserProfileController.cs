using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.UserProfileFeature.Commands;
using Olivaw.Identity.Api.Features.UserProfileFeature.Queries;

namespace Olivaw.Identity.Api.Features.UserProfileFeature;

[ApiController]
[Route("identity/userprofiles")]
[Authorize(Policy = "identity.manage")]
public sealed class UserProfileController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateUserProfileResponse>> Create(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateUserProfileResponse>> Update(Guid id, UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<DeleteUserProfileResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteUserProfileCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReadUserProfileByIdResponse>> Read(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadUserProfileByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<ReadUserProfileByFilterResponse>> Read([FromQuery] ReadUserProfileByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
