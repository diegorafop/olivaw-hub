using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Api.Features.HealthReadingFeature.Queries;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Queries;
namespace Owh.Olivaw.Api.Features.HealthReadingFeature;
[ApiController, Route("health-readings"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class HealthReadingController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpPost("http")] public async Task<IActionResult> CreateHttp([FromBody] CreateHttpHealthReadingCommand command, CancellationToken token)
    {
        var name = User.FindFirst("name")?.Value ?? "Operação Olivaw"; if (name.Length > 120) name = name[..120];
        var result = await mediator.Send(new CreateHttpHealthReadingMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = name }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(201, result.Response);
    }
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadHealthReadingByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadHealthReadingByFilterMediatorQuery { Payload = query }, token));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateHealthReadingCommand command, CancellationToken token)
    {
        var name = User.FindFirst("name")?.Value ?? "Operação Olivaw"; if (name.Length > 120) name = name[..120];
        var result = await mediator.Send(new CreateHealthReadingMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = name }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(201, result.Response);
    }
}
