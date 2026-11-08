using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpLogging;
using Olivaw.Identity.Api.Security;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature.Commands;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature.Queries;
using Owh.Olivaw.Client.Features.AwsInvoiceFeature.Commands;
using Owh.Olivaw.Client.Features.AwsInvoiceFeature.Queries;
namespace Owh.Olivaw.Api.Features.AwsInvoiceFeature;
[ApiController,Route("aws-invoices"),Authorize(Policy="olivaw.root")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class AwsInvoiceController(IMediator mediator,CurrentActor actor) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Read([FromQuery]ReadAwsInvoiceByFilterQuery query,CancellationToken token)=>Ok(await mediator.Send(new ReadAwsInvoiceByFilterMediatorQuery{Payload=query},token));
    [HttpGet("{id:guid}")] public async Task<IActionResult> ReadById(Guid id,CancellationToken token)=>Ok(await mediator.Send(new ReadAwsInvoiceByIdMediatorQuery{Payload=new(){ExternalId=id}},token));
    [HttpPost,RequestSizeLimit(1200000)] public async Task<IActionResult> Create([FromBody]CreateAwsInvoiceCommand command,CancellationToken token)
    {
        var result=await mediator.Send(new CreateAwsInvoiceMediatorCommand{Payload=command,ActorUserId=actor.Id},token);
        return result.Conflict!=null ? Conflict(new{detail=result.Conflict}) : StatusCode(201,result.Response);
    }
}
