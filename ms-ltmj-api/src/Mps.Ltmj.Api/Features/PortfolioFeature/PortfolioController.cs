using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mps.Ltmj.Api.Features.PortfolioFeature.Commands;
using Mps.Ltmj.Api.Features.PortfolioFeature.Queries;
using System.Security.Claims;

namespace Mps.Ltmj.Api.Features.PortfolioFeature;

[ApiController]
[Route("portfolio/events")]
[Authorize(Policy = "portfolio.manage")]
public sealed class PortfolioController(IMediator mediator, IAuthorizationService authorization) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue("sub")!);

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Public([FromQuery] ReadEventsByFilterQuery query, CancellationToken cancellationToken)
    {
        query.PublicOnly = true;
        Response.Headers.CacheControl = "no-store";
        return Ok(await mediator.Send(query, cancellationToken));
    }

    [HttpGet("management")]
    public async Task<IActionResult> Management([FromQuery] ReadEventsByFilterQuery query, CancellationToken cancellationToken)
    {
        query.PublicOnly = false;
        Response.Headers.CacheControl = "no-store";
        return Ok(await mediator.Send(query, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command, CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        command.UserId = UserId;
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [HttpPatch("{id:guid}/visibility")]
    public async Task<IActionResult> Visibility(Guid id, [FromBody] UpdateEventVisibilityCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        command.UserId = UserId;
        return Ok(await mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteEventCommand { Id = id, UserId = UserId }, cancellationToken));
    }

    [HttpPost("{id:guid}/photos")]
    [RequestSizeLimit(106954752)]
    [RequestFormLimits(MultipartBodyLengthLimit = 106954752)]
    public async Task<IActionResult> Upload(Guid id, [FromForm] List<IFormFile> files, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new UploadEventPhotosCommand { Id = id, Files = files, UserId = UserId }, cancellationToken));
    }

    [HttpGet("{eventId:guid}/photos/{photoId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Photo(Guid eventId, Guid photoId, CancellationToken cancellationToken)
    {
        var allowed = await authorization.AuthorizeAsync(User, "portfolio.manage");
        var response = await mediator.Send(new ReadEventPhotoQuery { EventId = eventId, PhotoId = photoId, CanManage = allowed.Succeeded }, cancellationToken);
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(response.Content, response.ContentType);
    }
}
