using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.CareEvidenceFeature.Commands;
using Owh.Olivaw.Api.Features.CareEvidenceFeature.Queries;
using Owh.Olivaw.Client.Features.CareEvidenceFeature.Commands;
using Owh.Olivaw.Client.Features.CareEvidenceFeature.Queries;
namespace Owh.Olivaw.Api.Features.CareEvidenceFeature;
[ApiController, Route("care-evidence"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class CareEvidenceController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadCareEvidenceByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadCareEvidenceByFilterMediatorQuery { Payload = query }, token));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateCareEvidenceCommand command, CancellationToken token)
    {
        var name = User.FindFirst("name")?.Value ?? "Operação Olivaw"; if (name.Length > 120) name = name[..120];
        var result = await mediator.Send(new CreateCareEvidenceMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = name }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(201, result.Response);
    }
}
