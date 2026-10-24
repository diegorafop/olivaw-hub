using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpLogging;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.InventoryFeature.Commands;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
namespace Owh.Olivaw.Api.Features.InventoryFeature;
[ApiController, Route("resource-customers"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class ResourceCustomerController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResourceCustomerCommand request, CancellationToken token)
    {
        var result = await mediator.Send(new CreateResourceCustomerMediatorCommand { Payload = request, ActorUserId = actor.Id, ActorName = ActorName() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(StatusCodes.Status201Created, result.Response);
    }
    [HttpPut("{externalId:guid}")]
    public async Task<IActionResult> Update(Guid externalId, [FromBody] UpdateResourceCustomerCommand request, CancellationToken token)
    {
        var result = await mediator.Send(new UpdateResourceCustomerMediatorCommand { ExternalId = externalId, Payload = request, ActorUserId = actor.Id, ActorName = ActorName() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return Ok(result.Response);
    }
    private string ActorName() { var name = User.FindFirst("name")?.Value ?? "Operação Olivaw"; if (name.Length > 120) return name[..120]; return name; }
}
