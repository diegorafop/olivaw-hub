using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.DeliveryNoteFeature.Commands;
using Owh.Olivaw.Api.Features.DeliveryNoteFeature.Queries;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Commands;
using Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
namespace Owh.Olivaw.Api.Features.DeliveryNoteFeature;
[ApiController, Route("delivery-notes"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class DeliveryNoteController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadDeliveryNoteByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadDeliveryNoteByFilterMediatorQuery { Payload = query }, token));
    [HttpGet("{id:guid}/history")] public async Task<IActionResult> ReadHistory(Guid id, [FromQuery] int beforeId, CancellationToken token) => Ok(await mediator.Send(new ReadDeliveryNoteHistoryMediatorQuery { Payload = new() { ExternalId = id, BeforeId = beforeId } }, token));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateDeliveryNoteCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreateDeliveryNoteMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict }); return StatusCode(201, result.Response);
    }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeliveryNoteCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new UpdateDeliveryNoteMediatorCommand { ExternalId = id, Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict }); return Ok(result.Response);
    }
    private string Name() { var value = User.FindFirst("name")?.Value ?? "Operação Olivaw"; if (value.Length > 120) return value[..120]; return value; }
}
