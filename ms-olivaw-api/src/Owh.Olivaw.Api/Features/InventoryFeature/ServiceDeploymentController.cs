using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpLogging;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.InventoryFeature.Commands;
using Owh.Olivaw.Api.Features.InventoryFeature.Queries;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
namespace Owh.Olivaw.Api.Features.InventoryFeature;
[ApiController, Route("deployments"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class ServiceDeploymentController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> ReadByFilter([FromQuery] ReadServiceDeploymentByFilterQuery request, CancellationToken token) => Ok(await mediator.Send(new ReadServiceDeploymentByFilterMediatorQuery { Payload = request }, token));
    [HttpGet("{externalId:guid}")] public async Task<IActionResult> ReadById(Guid externalId, CancellationToken token) => Ok(await mediator.Send(new ReadServiceDeploymentByIdMediatorQuery { Payload = new() { ExternalId = externalId } }, token));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateServiceDeploymentCommand request, CancellationToken token)
    {
        var result = await mediator.Send(new CreateServiceDeploymentMediatorCommand { Payload = request, ActorUserId = actor.Id, ActorName = ActorName() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return CreatedAtAction(nameof(ReadById), new { externalId = result.Response.ExternalId }, result.Response);
    }
    [HttpPut("{externalId:guid}")] public async Task<IActionResult> Update(Guid externalId, [FromBody] UpdateServiceDeploymentCommand request, CancellationToken token)
    {
        var result = await mediator.Send(new UpdateServiceDeploymentMediatorCommand { ExternalId = externalId, Payload = request, ActorUserId = actor.Id, ActorName = ActorName() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return Ok(result.Response);
    }
    private string ActorName() { var name = User.FindFirst("name")?.Value ?? "Operação Olivaw"; if (name.Length > 120) return name[..120]; return name; }
}
