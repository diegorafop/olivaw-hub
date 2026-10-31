using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.FinancialFeature.Commands;
using Owh.Olivaw.Api.Features.FinancialFeature.Queries;
using Owh.Olivaw.Client.Features.FinancialFeature.Commands;
using Owh.Olivaw.Client.Features.FinancialFeature.Queries;
namespace Owh.Olivaw.Api.Features.FinancialFeature;
[ApiController,Route("finance"),Authorize(Policy="olivaw.root")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class FinancialController(IMediator mediator,CurrentActor actor):ControllerBase
{
 [HttpGet("accounts")]public async Task<IActionResult> ReadAccounts([FromQuery]ReadFinancialAccountByFilterQuery query,CancellationToken token)=>Ok(await mediator.Send(new ReadFinancialAccountByFilterMediatorQuery{Payload=query},token));
 [HttpGet("entries")]public async Task<IActionResult> ReadEntries([FromQuery]ReadFinancialEntryByFilterQuery query,CancellationToken token)=>Ok(await mediator.Send(new ReadFinancialEntryByFilterMediatorQuery{Payload=query},token));
 [HttpGet("entries/{id:guid}")]public async Task<IActionResult> ReadEntry(Guid id,CancellationToken token)=>Ok(await mediator.Send(new ReadFinancialEntryByIdMediatorQuery{Payload=new(){ExternalId=id}},token));
 [HttpGet("summary")]public async Task<IActionResult> ReadSummary([FromQuery]ReadFinancialSummaryQuery query,CancellationToken token)=>Ok(await mediator.Send(new ReadFinancialSummaryMediatorQuery{Payload=query},token));
 [HttpPost("accounts")]public async Task<IActionResult> CreateAccount([FromBody]CreateFinancialAccountCommand command,CancellationToken token){var r=await mediator.Send(new CreateFinancialAccountMediatorCommand{Payload=command,ActorUserId=actor.Id},token);return r.Conflict!=null?Conflict(new{detail=r.Conflict}):StatusCode(201,r.Response);}
 [HttpPost("entries")]public async Task<IActionResult> CreateEntry([FromBody]CreateFinancialEntryCommand command,CancellationToken token){var r=await mediator.Send(new CreateFinancialEntryMediatorCommand{Payload=command,ActorUserId=actor.Id},token);return r.Conflict!=null?Conflict(new{detail=r.Conflict}):StatusCode(201,r.Response);}
 [HttpPost("settlements")]public async Task<IActionResult> CreateSettlement([FromBody]CreateFinancialSettlementCommand command,CancellationToken token){var r=await mediator.Send(new CreateFinancialSettlementMediatorCommand{Payload=command,ActorUserId=actor.Id},token);return r.Conflict!=null?Conflict(new{detail=r.Conflict}):StatusCode(201,r.Response);}
 [HttpPost("transfers")]public async Task<IActionResult> CreateTransfer([FromBody]CreateFinancialTransferCommand command,CancellationToken token){var r=await mediator.Send(new CreateFinancialTransferMediatorCommand{Payload=command,ActorUserId=actor.Id},token);return r.Conflict!=null?Conflict(new{detail=r.Conflict}):StatusCode(201,r.Response);}
 [HttpPatch("entries/{id:guid}")]public async Task<IActionResult> UpdateEntry(Guid id,[FromBody]UpdateFinancialEntryCommand command,CancellationToken token){command.ExternalId=id;var r=await mediator.Send(new UpdateFinancialEntryMediatorCommand{Payload=command,ActorUserId=actor.Id},token);return r.Conflict!=null?Conflict(new{detail=r.Conflict}):Ok(r.Response);}
}
