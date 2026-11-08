using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature.Commands;
using Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature.Queries;

namespace Olivaw.Identity.Api.Features.UserProfileAccessProfileFeature;

[ApiController]
[Route("identity/userprofileaccessprofiles")]
[Authorize(Policy = "identity.manage")]
public sealed class UserProfileAccessProfileController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateUserProfileAccessProfileResponse>> Create(CreateUserProfileAccessProfileCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateUserProfileAccessProfileResponse>> Update(Guid id, UpdateUserProfileAccessProfileCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<DeleteUserProfileAccessProfileResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteUserProfileAccessProfileCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReadUserProfileAccessProfileByIdResponse>> Read(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadUserProfileAccessProfileByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<ReadUserProfileAccessProfileByFilterResponse>> Read([FromQuery] ReadUserProfileAccessProfileByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
