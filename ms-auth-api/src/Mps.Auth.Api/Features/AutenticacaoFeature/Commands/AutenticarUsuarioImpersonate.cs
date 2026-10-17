using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Model;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.AutenticacaoFeature.Commands;

public class AutenticarImpersonateCommand
    : IRequest<AutenticarImpersonateCommandResponse>
{
    public string Login { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public IEnumerable<string>? AutorizacaoMany { get; set; }
}

public class AutenticarImpersonateCommandResponse
{
    public bool Ativo { get; set; }
    public Guid UsuarioId { get; set; }
    public string Token { get; set; }
    public string TokenRefresh { get; set; }
}

public class AutenticarImpersonateCommandHandler
(
    IConfiguration configuration,
    IMediator mediator,
    IRepositoryUsuario repository,
    IRepository<UsuarioAutenticacao> repositoryUsuarioAutenticacao,
    IRepositoryPerfilAcesso repositoryPerfilAcesso,
    IRepository<Autorizacao> repositoryAutorizacao,
    AutenticarImpersonateCommandValidator validator
) : IRequestHandler<AutenticarImpersonateCommand,
    AutenticarImpersonateCommandResponse>
{
    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    private readonly IRepository<UsuarioAutenticacao> _repositoryUsuarioAutenticacao = repositoryUsuarioAutenticacao
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioAutenticacao));

    private readonly IRepositoryPerfilAcesso _repositoryPerfilAcesso = repositoryPerfilAcesso
        ?? throw new ArgumentNullException(nameof(repositoryPerfilAcesso));

    private readonly IRepository<Autorizacao> _repositoryAutorizacao = repositoryAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryAutorizacao));

    private readonly AutenticarImpersonateCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AutenticarImpersonateCommandResponse> Handle
    (
        AutenticarImpersonateCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var usuarioModel = request.ToUsuarioModel();

        var entity = await GetUsuarioAsync
        (
            request,
            usuarioModel,
            cancellationToken
        );

        entity.Autorizacoes = await GetAutorizacaoOverrideManyAsync(request, entity, cancellationToken);

        var usuarioAutenticacao = await UpsertAsync(request, entity, cancellationToken);

        return entity
            .ToAutenticarImpersonateCommandResponse
            (
                usuarioAutenticacao.Token,
                _configuration
            );
    }

    private async Task<Usuario> GetUsuarioAsync
    (
        AutenticarImpersonateCommand request,
        UsuarioModel usuarioModel,
        CancellationToken cancellationToken
    )
    {
        var usuario = await GetFirstOrDefaultAsync(usuarioModel, cancellationToken);

        if (usuario is not null)
            return usuario;

        var command = request.ToInserirUsuarioInternalCommand();
        var response = await _mediator.Send(command, cancellationToken);

        return response.Usuario;
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

    private async Task<UsuarioAutenticacao> UpsertAsync
    (
        AutenticarImpersonateCommand request,
        Usuario? entity,
        CancellationToken cancellationToken
    )
    {
        UsuarioAutenticacao usuarioAutenticacao = await UpsertUsuarioAutenticacaoAsync
        (
            request,
            entity,
            cancellationToken
        );

        await _repository.SaveChangesAsync(cancellationToken);

        return usuarioAutenticacao;
    }

    public async Task<UsuarioAutenticacao> UpsertUsuarioAutenticacaoAsync
    (
        AutenticarImpersonateCommand request,
        Usuario? entity,
        CancellationToken cancellationToken
    )
    {
        if (entity is null)
        {
            var perfilAcesso = await _repositoryPerfilAcesso.GetFirstAsync
            (
                request.SearchPredicateByPerfilBasico(),
                cancellationToken,
                item => item.Autorizacaos
            );
            entity = request.ToDomain(perfilAcesso);
        }

        var usuarioAutenticacao = entity.ToUsuarioAutenticacao();

        await _repositoryUsuarioAutenticacao.AddAsync
        (
            usuarioAutenticacao,
            cancellationToken
        );

        return usuarioAutenticacao;
    }

    private async Task<Autorizacao> GetAutorizacaoOverrideAsync
    (
        AutenticarImpersonateCommand request,
        CancellationToken cancellationToken
    )
    {

        var autorizacaoOverride = await _repositoryAutorizacao.GetFirstAsync
        (
            request.SearchPredicateByRoleOverride(),
            cancellationToken
        );
        return autorizacaoOverride;
    }

    private async Task<IEnumerable<UsuarioAutorizacao>> GetAutorizacaoOverrideManyAsync
    (
        AutenticarImpersonateCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        List<UsuarioAutorizacao> itens = GetManyUsuarioAutorizacao(entity);

        Autorizacao autorizacaoOverride = await GetAutorizacaoOverrideAsync(request, cancellationToken);

        UsuarioAutorizacao usuarioAutorizacao = request.ToDomain(entity, autorizacaoOverride);
        itens.Add(usuarioAutorizacao);

        if (request.AutorizacaoMany == null)
            return itens;

        if (!request.AutorizacaoMany.Any())
            return itens;

        var predicate = request.SearchPredicateByList([.. request.AutorizacaoMany]);

        var autorizacoes = await _repositoryAutorizacao.GetAsync
        (
            predicate,
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            autorizacoes,
            cancellationToken
        );

        foreach (var autorizacao in autorizacoes)
        {
            usuarioAutorizacao = request.ToDomain(entity, autorizacao);
            itens.Add(usuarioAutorizacao);
        }
        return itens;
    }

    private static List<UsuarioAutorizacao> GetManyUsuarioAutorizacao
    (
        Usuario? entity
    )
    {
        if (entity is null)
            return [];

        if (entity.Autorizacoes is null)
            return [];

        return [.. entity.Autorizacoes];
    }
}

public class AutenticarImpersonateCommandValidator
    : AbstractValidator<AutenticarImpersonateCommand>
{
    public AutenticarImpersonateCommandValidator()
    {
        RuleFor(item => item.Login)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarImpersonateCommand, string>
                (item => item.Login))
            .NotNull()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarImpersonateCommand, string>
                (item => item.Login));

        RuleFor(item => item.Nome)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarImpersonateCommand, string>
                (item => item.Nome))
            .NotNull()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarImpersonateCommand, string>
                (item => item.Nome));

        RuleFor(item => item.Email)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarImpersonateCommand, string>
                (item => item.Email))
            .NotNull()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarImpersonateCommand, string>
                (item => item.Email));
    }

    public async Task ValidateDataAsync
    (
        AutenticarImpersonateCommand request,
        IEnumerable<Autorizacao> autorizacoes,
        CancellationToken cancellationToken
    )
    {
        HashSet<string> rolesEncontradas = [.. autorizacoes.Select(autorizacao => autorizacao.Valor)];

        string? roleNaoEncontrada = request
            .AutorizacaoMany?
            .Distinct()
            .FirstOrDefault(role => !rolesEncontradas.Contains(role));

        RuleFor(item => item.AutorizacaoMany)
            .Must(_ => string.IsNullOrEmpty(roleNaoEncontrada))
            .WithMessage(MessageHelper.NotFoundFor<Autorizacao, string>
            (
                item => item.Valor,
                roleNaoEncontrada ?? string.Empty
            ));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}