using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.CustomerFeature.Commands;
using Owh.Olivaw.Api.Features.CustomerFeature.Queries;
using Owh.Olivaw.Client.Features.CustomerFeature.Commands;
using Owh.Olivaw.Client.Features.CustomerFeature.Queries;

namespace Owh.Olivaw.Api.Features.CustomerFeature;

[ApiController]
[Route("customers")]
[Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class CustomerController(IMediator mediator, CurrentActor actor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ReadByFilter([FromQuery] ReadCustomerByFilterQuery request, CancellationToken cancellationToken) => Ok(await mediator.Send(new ReadCustomerByFilterMediatorQuery { Payload = request }, cancellationToken));
    [HttpGet("summary")]
    public async Task<IActionResult> ReadSummary(CancellationToken cancellationToken) => Ok(await mediator.Send(new ReadCustomerSummaryMediatorQuery(), cancellationToken));
    [HttpGet("{externalId:guid}")]
    public async Task<IActionResult> ReadById(Guid externalId, CancellationToken cancellationToken) => Ok(await mediator.Send(new ReadCustomerByIdMediatorQuery { Payload = new ReadCustomerByIdQuery { ExternalId = externalId } }, cancellationToken));
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCustomerMediatorCommand { Payload = request, ActorUserId = actor.Id, ActorName = ActorName() }, cancellationToken);
        if (result.Conflict != null) return Conflict(new { title = "Confira o cadastro", detail = result.Conflict, existingExternalId = result.ExistingExternalId });
        return CreatedAtAction(nameof(ReadById), new { externalId = result.Response.ExternalId }, result.Response);
    }
    [HttpPut("{externalId:guid}")]
    public async Task<IActionResult> Update(Guid externalId, [FromBody] UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateCustomerMediatorCommand { ExternalId = externalId, Payload = request, ActorUserId = actor.Id, ActorName = ActorName() }, cancellationToken);
        if (result.Conflict != null) return Conflict(new { title = "Confira o cadastro", detail = result.Conflict });
        return Ok(result.Response);
    }
    private string ActorName()
    {
        var name = User.FindFirst("name")?.Value ?? "Operação Olivaw";
        return name.Length > 120 ? name[..120] : name;
    }
}
