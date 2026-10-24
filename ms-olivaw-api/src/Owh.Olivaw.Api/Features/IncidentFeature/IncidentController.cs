using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.IncidentFeature.Commands;
using Owh.Olivaw.Api.Features.IncidentFeature.Queries;
using Owh.Olivaw.Client.Features.IncidentFeature.Commands;
using Owh.Olivaw.Client.Features.IncidentFeature.Queries;
namespace Owh.Olivaw.Api.Features.IncidentFeature;
[ApiController, Route("incidents"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class IncidentController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadIncidentByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadIncidentByFilterMediatorQuery { Payload = query }, token));
    [HttpGet("{externalId:guid}/history")] public async Task<IActionResult> ReadHistory(Guid externalId, [FromQuery] int beforeId, CancellationToken token) => Ok(await mediator.Send(new ReadIncidentHistoryMediatorQuery { Payload = new() { ExternalId = externalId, BeforeId = beforeId } }, token));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateIncidentCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreateIncidentMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(201, result.Response);
    }
    [HttpPut("{externalId:guid}")] public async Task<IActionResult> Update(Guid externalId, [FromBody] UpdateIncidentCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new UpdateIncidentMediatorCommand { ExternalId = externalId, Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return Ok(result.Response);
    }
    private string Name() { var name = User.FindFirst("name")?.Value ?? "Operação Olivaw"; if (name.Length > 120) return name[..120]; return name; }
}
