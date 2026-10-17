using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Api.Features.UsuarioFeature.Queries;
using Mps.HolosCore.Api.Extensions.Https;

namespace Mps.Auth.Api.Features.UsuarioFeature;

[Route("usuario")]
[ApiController]
public class UsuarioController
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
        [FromRoute] SelecionarUsuarioByIdQuery request,
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
        [FromQuery] SelecionarUsuarioByFilterQuery request,
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

    // TODO: verificar uma forma de possibilitar a criação de usuário por outros serviços sem necessidade de
    // token do usuário. Isso é aplicado no caso da confirmação de email no cadastro de usuário
    //[Authorize(Roles = "root, Usuario")]
    [HttpGet("verificar")]
    [AllowAnonymous]
    public async Task<ActionResult> Get
    (
        [FromQuery] VerificarExistenciaUsuarioQuery request,
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
    [AllowAnonymous]
    public async Task<ActionResult> Post
    (
        [FromBody] InserirUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }
    // end TODO

    [HttpPost("upsert")]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Post
    (
        [FromBody] UpsertUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPost("upsert-usuario-holos")]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Post
    (
        [FromBody] UpsertUsuarioHolosCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut("perfil")]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarPerfilCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut("senha")]
    public async Task<ActionResult> Put
    (
        [FromBody] AlterarSenhaCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut("externo/senha")]
    [AllowAnonymous]
    public async Task<ActionResult> Put
    (
        [FromBody] AlterarSenhaByExternoCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId
        (
            _configuration,
            request.UsuarioId
        );

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut("perfil/senha")]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Put
    (
        [FromBody] AlterarPerfilSenhaCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpDelete($"{{id}}")]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Delete
    (
        [FromRoute] RemoverUsuarioCommand request,
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

    [HttpPut("alterar")]
    [Authorize(Roles = "root, Usuario")]
    public async Task<ActionResult> Alternar
    (
        [FromBody] AlternarUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPost("recuperar-senha")]
    [AllowAnonymous]
    public async Task<ActionResult> RecuperarSenha
    (
        [FromBody] RecuperarSenhaCommand request,
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

    //TODO - corrigir para padrão rest, matar essa rota depois de verificar onde é usada
    [HttpGet("many-by-id-many")]
    public async Task<ActionResult> GetOld
    (
        [FromQuery] SelecionarUsuarioByIdManyQuery request
    )
    {
        return Ok(await _mediator.Send(request));
    }

    [HttpPut("status-integracao-enum")]
    [Authorize(Roles = "root")]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarUsuarioStatusIntegracaoEnumPendenteCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut("status-integracao-enum/pendente-processo-admissao")]
    [Authorize(Roles = "root")]
    public async Task<ActionResult> Put
    (
        [FromBody] AtualizarUsuarioStatusIntegracaoEnumPendenteProcessoAdmissaoCommand request,
        CancellationToken cancellationToken
    )
    {
        request.UsuarioLogadoId = HttpContext.GetUsuarioId(_configuration);

        var response = await _mediator.Send
        (
            request,
            cancellationToken
        );

        return Ok(response);
    }
}