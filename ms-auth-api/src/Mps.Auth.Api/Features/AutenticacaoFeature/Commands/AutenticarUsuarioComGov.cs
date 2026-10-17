using FluentValidation;
using MassTransit;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Model;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Text.Json.Serialization;

namespace Mps.Auth.Api.Features.AutenticacaoFeature.Commands;

public class AutenticarUsuarioComGovCommand
    : EntityCommand,
      IRequest<AutenticarUsuarioComGovCommandResponse>
{
    internal string IdToken { get; set; }
    internal string NivelConta { get; set; }

    public AutenticarUsuarioComGovCommand() { }
    public AutenticarUsuarioComGovCommand
    (
        string idToken,
        Guid usuarioLogadoId
    )
        : base(usuarioLogadoId)
    {
        IdToken = idToken;
    }
}

public class AutenticarUsuarioComGovCommandResponse
{
    public string Token { get; set; }
    public string TokenRefresh { get; set; }
}

public class ReliabilityInfo
{
    [JsonPropertyName("level")]
    public string Level { get; set; }

    [JsonPropertyName("reliabilities")]
    public List<ReliabilityDetail> Reliabilities { get; set; }
}

public class ReliabilityDetail
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}

public class AutenticarUsuarioComGovCommandHandler
(
    IPublishEndpoint publisher,
    IConfiguration configuration,
    IMediator mediator,
    IRepositoryUsuario repository,
    IRepository<UsuarioAutenticacao> repositoryUsuarioAutenticacao,
    IRepository<UsuarioAutorizacao> repositoryUsuarioAutorizacao,
    IRepository<Autorizacao> repositoryAutorizacao
) : IRequestHandler<AutenticarUsuarioComGovCommand,
    AutenticarUsuarioComGovCommandResponse>
{
    private readonly IPublishEndpoint _publisher = publisher
        ?? throw new ArgumentNullException(nameof(publisher));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepository<UsuarioAutenticacao> _repositoryUsuarioAutenticacao = repositoryUsuarioAutenticacao
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioAutenticacao));

    private readonly IRepository<UsuarioAutorizacao> _repositoryUsuarioAutorizacao = repositoryUsuarioAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioAutorizacao));

    private readonly IRepository<Autorizacao> _repositoryAutorizacao = repositoryAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryAutorizacao));

    public async Task<AutenticarUsuarioComGovCommandResponse> Handle
    (
        AutenticarUsuarioComGovCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var usuarioModel = request.ToUsuarioModel(_configuration);

        Usuario entity = await GetUsuarioAsync
        (
            request,
            usuarioModel,
            cancellationToken
        );

        var usuarioAutenticacao = await UpsertAsync
        (
            request,
            entity,
            cancellationToken
        );

        return entity.ToAutenticarUsuarioComGovCommandResponse
        (
            usuarioAutenticacao.Token,
            _configuration
        );
    }

    private async Task<Usuario> GetUsuarioAsync
    (
        AutenticarUsuarioComGovCommand request,
        UsuarioModel usuarioModel,
        CancellationToken cancellationToken
    )
    {
        var usuario = await GetFirstOrDefaultAsync(usuarioModel, cancellationToken);

        if (usuario is not null)
            return usuario;

        var command = request.ToInserirUsuarioInternalCommand(_configuration);
        var response = await _mediator.Send(command, cancellationToken);

        Autorizacao autorizacao = await GetAutorizacaoNivelContaAsync(request, cancellationToken);
        var itens = request.ToDomain(response, autorizacao);

        await _repositoryUsuarioAutorizacao.AddCollectionAsync
        (
            itens,
            cancellationToken
        );

        return response.Usuario;
    }

    private async Task<Autorizacao> GetAutorizacaoNivelContaAsync
    (
        AutenticarUsuarioComGovCommand request,
        CancellationToken cancellationToken
    )
    {
        var nivelContaCapitalized = TextHelper.FirstCharToUpper(request.NivelConta);

        var autorizacaoNivelConta = await _repositoryAutorizacao.GetFirstAsync
        (
            request.SearchPredicateByRoleNivelConta(nivelContaCapitalized),
            cancellationToken
        );

        return autorizacaoNivelConta;
    }

    private async Task<UsuarioAutenticacao> UpsertAsync
    (
        AutenticarUsuarioComGovCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        UsuarioAutenticacao usuarioAutenticacao = await UpsertUsuarioAsync
        (
            request,
            entity,
            cancellationToken
        );

        await _repository.SaveChangesAsync(cancellationToken);

        return usuarioAutenticacao;
    }

    private async Task<Usuario?> GetFirstOrDefaultAsync
    (
        UsuarioModel usuarioModel,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstOrDefaultAsync
        (
            item => item
                .Login
                .Equals(usuarioModel.Login),
            cancellationToken,
            item => item.Autorizacoes,
            item => item.Perfils
        );
    }

    public async Task<UsuarioAutenticacao> UpsertUsuarioAsync
    (
        AutenticarUsuarioComGovCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        var usuarioAutenticacao = entity.ToUsuarioAutenticacao();

        if (entity.Id != 0)
        {
            entity.Update(request, _configuration);
            await _repository.UpdateAsync(entity, cancellationToken);
        }
        else
            await EnfileirarUpsertDestinatarioAsync(entity, cancellationToken);

        await _repositoryUsuarioAutenticacao.AddAsync
        (
            usuarioAutenticacao,
            cancellationToken
        );

        await _repositoryUsuarioAutenticacao.SaveChangesAsync(cancellationToken);

        return usuarioAutenticacao;
    }

    private async Task EnfileirarUpsertDestinatarioAsync
    (
        Usuario usuario,
        CancellationToken cancellationToken
    )
    {
        var contract = usuario.ToEnfileirarUpsertDestinatarioContract();
        await _publisher.Publish(contract, cancellationToken);
    }
}

public class AutenticarUsuarioComGovCommandValidator
    : AbstractValidator<AutenticarUsuarioComGovCommand>
{
    public AutenticarUsuarioComGovCommandValidator()
    {
        RuleFor(item => item.IdToken)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarUsuarioComGovCommand, string>
                (item => item.IdToken));
    }

    public async Task ValidateDataAsync
    (
        AutenticarUsuarioComGovCommand request,
        UsuarioModel usuarioModel,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.IdToken)
            .Must(item => !string.IsNullOrEmpty(usuarioModel.Nome))
            .WithMessage(MessageHelper
                .EmptyFor<Usuario, string>
                (item => item.Nome));

        RuleFor(item => item.IdToken)
            .Must(item => !string.IsNullOrEmpty(usuarioModel.Email))
            .WithMessage(MessageHelper
                .EmptyFor<Usuario, string>
                (item => item.Email));

        RuleFor(item => item.IdToken)
            .Must(item => !string.IsNullOrEmpty(usuarioModel.Login))
            .WithMessage(MessageHelper
                .EmptyFor<Usuario, string>
                (item => item.Login));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}