using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mps.HolosCore.Api.Extensions.Https;
using Mps.Limpo.Api.Features.ExampleFeature.Commands;
using Mps.Limpo.Api.Features.ExampleFeature.Queries;

namespace Mps.Limpo.Api.Features.ExampleFeature;

[Route("example")]
[ApiController]
public class ExampleController
(
    IMediator mediator,
    IConfiguration configuration
) : ControllerBase
{
    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    [HttpGet($"{{id}}")]
    public async Task<ActionResult> Get
    (
        [FromRoute] SelecionarExampleByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult> Get
    (
        [FromQuery] SelecionarExampleByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> Post
    (
        [FromBody] InserirExampleCommand request
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId
        (
            _configuration,
            request.UsuarioLogadoId
        );

        return Ok(await _mediator.Send(request));
    }

    [HttpPut]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId
        (
            _configuration,
            request.UsuarioLogadoId
        );

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpDelete($"{{id}}")]
    public async Task<ActionResult> Delete
    (
        [FromRoute] RemoverExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut("alternar")]
    public async Task<ActionResult> Put
    (
        [FromBody] AlternarExampleCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId
        (
            _configuration,
            request.UsuarioLogadoId
        );

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }
}
