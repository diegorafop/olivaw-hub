using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.ExpenseFeature.Commands;
using Owh.Olivaw.Api.Features.ExpenseFeature.Queries;
using Owh.Olivaw.Client.Features.ExpenseFeature.Commands;
using Owh.Olivaw.Client.Features.ExpenseFeature.Queries;
namespace Owh.Olivaw.Api.Features.ExpenseFeature;
[ApiController, Route("expenses"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ExpenseController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Read([FromQuery] ReadExpenseByFilterQuery query, CancellationToken token) => Ok(await mediator.Send(new ReadExpenseByFilterMediatorQuery { Payload = query }, token));
    [HttpPost] public async Task<IActionResult> Create([FromBody] CreateExpenseCommand command, CancellationToken token)
    {
        var result = await mediator.Send(new CreateExpenseMediatorCommand { Payload = command, ActorUserId = actor.Id }, token);
        if (result.Conflict != null) return Conflict(new { detail = result.Conflict });
        return StatusCode(201, result.Response);
    }
}
