using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.UsageReadingFeature.Commands;
using Owh.Olivaw.Api.Features.UsageReadingFeature.Queries;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Commands;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Queries;
namespace Owh.Olivaw.Api.Features.UsageReadingFeature;
[ApiController, Route("usage-readings"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class UsageReadingController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadUsageReadingByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadUsageReadingByFilterMediatorQuery { Payload = query }, token));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateUsageReadingCommand command, CancellationToken token)
    {
        var name = User.FindFirst("name")?.Value ?? "Operação Olivaw"; if (name.Length > 120) name = name[..120];
        var result = await mediator.Send(new CreateUsageReadingMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = name }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(201, result.Response);
    }
}
