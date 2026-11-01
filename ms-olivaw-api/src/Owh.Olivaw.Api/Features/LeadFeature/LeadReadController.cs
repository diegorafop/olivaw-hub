using MediatR;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Owh.Olivaw.Api.Features.LeadFeature.Queries;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;

namespace Owh.Olivaw.Api.Features.LeadFeature;

[ApiController]
[Route("leads")]
[Authorize(Policy = "leads.manage")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class LeadReadController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(65536)]
    public async Task<IActionResult> Create([FromBody] CreateLeadCommand request, [FromServices] CurrentActor actor, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateLeadMediatorCommand { Payload = request, ActorUserId = actor.Id }, cancellationToken);
        if (result.Conflict) return Conflict(new { detail = "Este identificador já está cadastrado. Confira a lista antes de repetir." });
        return Created($"/leads/{result.ExternalId}", result);
    }
    [HttpPut("{externalId:guid}/proposal")]
    [RequestSizeLimit(65536)]
    public async Task<IActionResult> UpdateProposal(Guid externalId, [FromBody] UpdateLeadProposalCommand request, [FromServices] CurrentActor actor, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateLeadProposalMediatorCommand { ExternalId = externalId, Payload = request, ActorUserId = actor.Id }, cancellationToken);
        if (result.Conflict) return Conflict(new { detail = "A pasta mudou. Recarregue e confira a proposta antes de salvar." });
        return Ok(result);
    }
    [HttpGet]
    public async Task<IActionResult> ReadByFilter([FromQuery] ReadLeadByFilterQuery request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadLeadByFilterMediatorQuery { Payload = request }, cancellationToken));
    }
    [HttpGet("{externalId:guid}")]
    public async Task<IActionResult> ReadById(Guid externalId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadLeadByIdMediatorQuery { Payload = new ReadLeadByIdQuery { ExternalId = externalId } }, cancellationToken));
    }
    [HttpPut("{externalId:guid}/follow-up")]
    public async Task<IActionResult> UpdateFollowUp(Guid externalId, [FromBody] UpdateLeadFollowUpCommand request, [FromServices] CurrentActor actor, CancellationToken cancellationToken)
    {
        var name = User.FindFirst("name")?.Value ?? "Operação Olivaw";
        if (name.Length > 120) name = name[..120];
        var result = await mediator.Send(new UpdateLeadFollowUpMediatorCommand
        {
            ExternalId = externalId, Payload = request, ActorUserId = actor.Id, ActorName = name
        }, cancellationToken);
        if (result.Conflict) return Conflict(new { detail = "Este lead mudou. Recarregue a ficha e confira antes de salvar novamente." });
        return Ok(result.Response);
    }
    [HttpGet("{externalId:guid}/history")]
    public async Task<IActionResult> ReadHistory(Guid externalId, [FromQuery] long? beforeRevision, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadLeadHistoryMediatorQuery
        {
            Payload = new() { ExternalId = externalId, BeforeRevision = beforeRevision }
        }, cancellationToken));
    }
    [HttpGet("summary")]
    public async Task<IActionResult> ReadSummary(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new ReadLeadSummaryMediatorQuery(), cancellationToken));
    }
}
