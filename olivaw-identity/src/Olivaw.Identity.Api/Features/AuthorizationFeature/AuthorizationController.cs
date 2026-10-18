using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.AuthorizationFeature.Commands;
using Olivaw.Identity.Api.Features.AuthorizationFeature.Queries;

namespace Olivaw.Identity.Api.Features.AuthorizationFeature;

[ApiController]
[Route("identity/authorizations")]
[Authorize(Policy = "identity.manage")]
public sealed class AuthorizationController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateAuthorizationResponse>> Create(CreateAuthorizationCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateAuthorizationResponse>> Update(Guid id, UpdateAuthorizationCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<DeleteAuthorizationResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteAuthorizationCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReadAuthorizationByIdResponse>> Read(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadAuthorizationByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<ReadAuthorizationByFilterResponse>> Read([FromQuery] ReadAuthorizationByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
