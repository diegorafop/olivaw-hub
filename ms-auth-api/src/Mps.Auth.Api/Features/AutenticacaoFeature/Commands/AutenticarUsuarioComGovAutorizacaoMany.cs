using FluentValidation;
using MassTransit;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Model;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.AutenticacaoFeature.Commands;

public class AutenticarUsuarioComGovAutorizacaoManyCommand
    : IRequest<AutenticarUsuarioComGovAutorizacaoManyCommandResponse>
{
    internal string IdToken { get; set; }
    public IEnumerable<string> AutorizacaoMany { get; set; }
}

public class AutenticarUsuarioComGovAutorizacaoManyCommandResponse
{
    public string Token { get; set; }
    public string TokenRefresh { get; set; }
}

public class AutenticarUsuarioComGovAutorizacaoManyCommandHandler
(
    IPublishEndpoint publisher,
    IConfiguration configuration,
    IMediator mediator,
    IRepositoryUsuario repository,
    IRepository<Autorizacao> repositoryAutorizacao,
    IRepository<UsuarioAutorizacao> repositoryUsuarioAutorizacao,
    IRepository<UsuarioAutenticacao> repositoryUsuarioAutenticacao
) : IRequestHandler<AutenticarUsuarioComGovAutorizacaoManyCommand,
    AutenticarUsuarioComGovAutorizacaoManyCommandResponse>
{
    private readonly IPublishEndpoint _publisher = publisher
        ?? throw new ArgumentNullException(nameof(publisher));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepository<Autorizacao> _repositoryAutorizacao = repositoryAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryAutorizacao));

    private readonly IRepository<UsuarioAutorizacao> _repositoryUsuarioAutorizacao = repositoryUsuarioAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioAutorizacao));

    private readonly IRepository<UsuarioAutenticacao> _repositoryUsuarioAutenticacao = repositoryUsuarioAutenticacao
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioAutenticacao));

    public async Task<AutenticarUsuarioComGovAutorizacaoManyCommandResponse> Handle
    (
        AutenticarUsuarioComGovAutorizacaoManyCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var usuarioModel = request.ToUsuarioModel(_configuration);

        var entity = await GetUsuarioAsync
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

        return entity.ToAutenticarUsuarioComGovAutorizacaoManyCommandResponse
        (
            usuarioAutenticacao.Token,
            _configuration
        );
    }

    private async Task UpsertAutorizacaoManyAsync
    (
        AutenticarUsuarioComGovAutorizacaoManyCommand request,
        Usuario usuario,
        CancellationToken cancellationToken
    )
    {
        if (usuario.Id != 0)
        {
            await _repositoryUsuarioAutorizacao.RemoveCollectionAsync
            (
                usuario.Autorizacoes.Where(item => item.CanBeRemoved(_configuration)),
                cancellationToken
            );
        }

        IEnumerable<Autorizacao> autorizacaoMany = await GetAutorizacaoManyAsync(request, cancellationToken);
        var itens = request.ToDomain(usuario, autorizacaoMany);

        await _repositoryUsuarioAutorizacao.AddCollectionAsync
        (
            itens,
            cancellationToken
        );
    }

    private async Task<Usuario> GetUsuarioAsync
    (
        AutenticarUsuarioComGovAutorizacaoManyCommand request,
        UsuarioModel usuarioModel,
        CancellationToken cancellationToken
    )
    {
        var usuario = await GetAsync(usuarioModel, cancellationToken);

        if (usuario is null)
        {
            var command = request.ToInserirUsuarioInternalCommand(_configuration);
            var response = await _mediator.Send(command, cancellationToken);
            return response.Usuario;
        }

        return usuario;
    }

    private async Task<Usuario?> GetAsync
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

    private async Task<IEnumerable<Autorizacao>> GetAutorizacaoManyAsync
    (
        AutenticarUsuarioComGovAutorizacaoManyCommand request,
        CancellationToken cancellationToken
    )
    {
        IList<Autorizacao> autorizacaoMany = [];

        foreach (var item in request.AutorizacaoMany)
        {
            var autorizacaoCapitalized = TextHelper.FirstCharToUpper(item);

            var autorizacao = await _repositoryAutorizacao.GetFirstAsync
            (
                request.SearchPredicateByRoleNivelConta(autorizacaoCapitalized),
                cancellationToken
            );

            autorizacaoMany.Add(autorizacao);
        }

        return autorizacaoMany;
    }

    private async Task<UsuarioAutenticacao> UpsertAsync
    (
        AutenticarUsuarioComGovAutorizacaoManyCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        await UpsertAutorizacaoManyAsync
        (
            request,
            entity,
            cancellationToken
        );

        UsuarioAutenticacao usuarioAutenticacao = await UpsertUsuarioAsync
        (
            request,
            entity,
            cancellationToken
        );

        await _repository.SaveChangesAsync(cancellationToken);

        return usuarioAutenticacao;
    }

    public async Task<UsuarioAutenticacao> UpsertUsuarioAsync
    (
        AutenticarUsuarioComGovAutorizacaoManyCommand request,
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

public class AutenticarUsuarioComGovAutorizacaoManyCommandValidator
    : AbstractValidator<AutenticarUsuarioComGovAutorizacaoManyCommand>
{
    public AutenticarUsuarioComGovAutorizacaoManyCommandValidator()
    {
        RuleFor(item => item.IdToken)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarUsuarioComGovAutorizacaoManyCommand, string>
                (item => item.IdToken));
    }
}