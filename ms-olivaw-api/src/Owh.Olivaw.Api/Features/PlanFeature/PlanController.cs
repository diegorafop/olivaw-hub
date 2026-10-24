using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.PlanFeature.Commands;
using Owh.Olivaw.Api.Features.PlanFeature.Queries;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
namespace Owh.Olivaw.Api.Features.PlanFeature;
[ApiController, Route("plans"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class PlanController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet("studies")] public async Task<IActionResult> ReadStudies([FromQuery] ReadPlanStudyByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPlanStudyByFilterMediatorQuery { Payload = query }, token));
    [HttpPost("studies")] public async Task<IActionResult> CreateStudy([FromBody] CreatePlanStudyCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreatePlanStudyMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(201, result.Response);
    }
    [HttpGet("costs")] public async Task<IActionResult> ReadCosts([FromQuery] ReadCostReferenceByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadCostReferenceByFilterMediatorQuery { Payload = query }, token));
    [HttpGet("conditions")] public async Task<IActionResult> ReadConditions([FromQuery] ReadCustomerPlanByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadCustomerPlanByFilterMediatorQuery { Payload = query }, token));
    [HttpPost("estimate")] public async Task<IActionResult> ReadEstimate([FromBody] ReadPlanEstimateQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadPlanEstimateMediatorQuery { Payload = query }, token));
    [HttpPost("costs")] public async Task<IActionResult> CreateCost([FromBody] CreateCostReferenceCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreateCostReferenceMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(201, result.Response);
    }
    [HttpPost("conditions")] public async Task<IActionResult> CreateCondition([FromBody] CreateCustomerPlanCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreateCustomerPlanMediatorCommand { Payload = command, ActorUserId = actor.Id, ActorName = Name() }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(201, result.Response);
    }
    private string Name()
    {
        var name = User.FindFirst("name")?.Value ?? "Operação Olivaw";
        if (name.Length > 120) return name[..120];
        return name;
    }
}
