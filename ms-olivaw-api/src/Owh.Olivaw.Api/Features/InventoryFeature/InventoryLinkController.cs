using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpLogging;
using Owh.Olivaw.Api.Features.InventoryFeature.Queries;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
namespace Owh.Olivaw.Api.Features.InventoryFeature;
[ApiController, Route("inventory-links"), Authorize(Policy = "olivaw.root")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class InventoryLinkController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ReadByFilter([FromQuery] ReadInventoryLinkByFilterQuery request, CancellationToken token) => Ok(await mediator.Send(new ReadInventoryLinkByFilterMediatorQuery { Payload = request }, token));
}
