using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature.Commands;
using Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature.Queries;

namespace Olivaw.Identity.Api.Features.AccessProfileAuthorizationFeature;

[ApiController]
[Route("identity/accessprofileauthorizations")]
[Authorize(Policy = "identity.manage")]
public sealed class AccessProfileAuthorizationController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateAccessProfileAuthorizationResponse>> Create(CreateAccessProfileAuthorizationCommand request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateAccessProfileAuthorizationResponse>> Update(Guid id, UpdateAccessProfileAuthorizationCommand request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<DeleteAccessProfileAuthorizationResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteAccessProfileAuthorizationCommand { Id = id }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReadAccessProfileAuthorizationByIdResponse>> Read(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadAccessProfileAuthorizationByIdQuery { Id = id }, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<ReadAccessProfileAuthorizationByFilterResponse>> Read([FromQuery] ReadAccessProfileAuthorizationByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
