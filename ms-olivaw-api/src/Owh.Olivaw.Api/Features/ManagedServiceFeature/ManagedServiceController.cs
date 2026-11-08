using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Commands;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Commands;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Queries;

namespace Owh.Olivaw.Api.Features.ManagedServiceFeature;

[ApiController]
[Route("services")]
[Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class ManagedServiceController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ReadByFilter([FromQuery] ReadManagedServiceByFilterQuery request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ReadManagedServiceByFilterMediatorQuery { Payload = request }, cancellationToken));
    [HttpGet("{externalId:guid}")]
    public async Task<IActionResult> ReadById(Guid externalId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ReadManagedServiceByIdMediatorQuery { Payload = new() { ExternalId = externalId } }, cancellationToken));
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManagedServiceCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateManagedServiceMediatorCommand { Payload = request, ActorUserId = actor.Id, ActorName = ActorName() }, cancellationToken);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return CreatedAtAction(nameof(ReadById), new { externalId = result.Response.ExternalId }, result.Response);
    }
    [HttpPut("{externalId:guid}")]
    public async Task<IActionResult> Update(Guid externalId, [FromBody] UpdateManagedServiceCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateManagedServiceMediatorCommand { ExternalId = externalId, Payload = request, ActorUserId = actor.Id, ActorName = ActorName() }, cancellationToken);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return Ok(result.Response);
    }
    private string ActorName()
    {
        var name = User.FindFirst("name")?.Value ?? "Operação Olivaw";
        if (name.Length > 120) return name[..120];
        return name;
    }
}
