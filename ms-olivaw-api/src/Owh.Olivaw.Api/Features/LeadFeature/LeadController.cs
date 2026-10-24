using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.RateLimiting;
using Owh.Olivaw.Api.Features.LeadFeature.Commands;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;

namespace Owh.Olivaw.Api.Features.LeadFeature;

[ApiController]
[Route("leads")]
[HttpLogging(HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode)]
public sealed class LeadController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpPut("{externalId:guid}")]
    [AllowAnonymous]
    [EnableRateLimiting("lead-write")]
    [RequestSizeLimit(65536)]
    public async Task<IActionResult> Update(Guid externalId, [FromHeader(Name = "X-Lead-Key")] string writeKey,
        [FromBody] UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateLeadMediatorCommand { ExternalId = externalId, WriteKey = writeKey, Payload = request }, cancellationToken);
        return result.Outcome switch
        {
            LeadWriteOutcome.Forbidden => StatusCode(StatusCodes.Status403Forbidden),
            LeadWriteOutcome.Conflict => Conflict(),
            _ => Ok(result.Response)
        };
    }
}
