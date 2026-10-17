using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Helpers;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using Mps.HolosCore.Requests;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.AutenticacaoFeature.Commands;

public class AutenticarUsuarioExternalCommand
    : AutenticarUsuarioCommand,
      IRequest<AutenticarUsuarioExternalCommandResponse>
{
    public string? Email { get; set; }
}

public class AutenticarUsuarioExternalCommandResponse
    : AutenticarUsuarioCommandResponse
{ }

public class AutenticarUsuarioExternalCommandHandler
(
    IConfiguration configuration,
    IRepositoryUsuario repository,
    IRepository<UsuarioAutenticacao> repositoryUsuarioAutenticacao,
    AutenticarUsuarioExternalCommandValidator validator
) : IRequestHandler<AutenticarUsuarioExternalCommand,
    AutenticarUsuarioExternalCommandResponse>
{
    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepository<UsuarioAutenticacao> _repositoryUsuarioAutenticacao = repositoryUsuarioAutenticacao
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioAutenticacao));

    private readonly AutenticarUsuarioExternalCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AutenticarUsuarioExternalCommandResponse> Handle
    (
        AutenticarUsuarioExternalCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Usuario entity = await GetFirstAsync(request, cancellationToken);

        UsuarioAutenticacao usuarioAutenticacao = entity.ToUsuarioAutenticacao();
        await AddAsync(usuarioAutenticacao, cancellationToken);

        return entity.ToAutenticarUsuarioExternalCommandResponse
        (
            usuarioAutenticacao.Token,
            _configuration
        );
    }

    private async Task<Usuario> GetFirstAsync
    (
        AutenticarUsuarioExternalCommand request,
        CancellationToken cancellationToken
    )
    {
        Usuario entity = await _repository.GetFirstAsync
        (
            SearchPredicate(request),
            cancellationToken,
            item => item.Autorizacoes,
            item => item.Perfils
        );

        await _validator.ValidateDataAsync
        (
            request,
            entity,
            cancellationToken
        );

        return entity;
    }

    private static Expression<Func<Usuario, bool>>
        SearchPredicate
    (
        AutenticarUsuarioExternalCommand request
    )
    {
        if (!string.IsNullOrEmpty(request.Login))
            return item => item
                .Login
                .Equals(request.Login);

        return item => item
            .Email
            .Equals(request.Email);
    }

    private async Task AddAsync
    (
        UsuarioAutenticacao usuarioAutenticacao,
        CancellationToken cancellationToken
    )
    {
        await _repositoryUsuarioAutenticacao.AddAsync
        (
            usuarioAutenticacao,
            cancellationToken
        );

        await _repositoryUsuarioAutenticacao.SaveChangesAsync(cancellationToken);
    }
}

public class AutenticarUsuarioExternalCommandValidator
    : AbstractValidator<AutenticarUsuarioExternalCommand>
{
    public AutenticarUsuarioExternalCommandValidator()
    {
        RuleFor(item => item.Login)
            .NotEmpty()
            .When(item => string.IsNullOrEmpty(item.Email))
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarUsuarioExternalCommand, string>
                (item => item.Login));

        RuleFor(item => item.Email)
            .NotEmpty()
            .When(item => string.IsNullOrEmpty(item.Login))
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarUsuarioExternalCommand, string>
                (item => item.Email));

        RuleFor(item => item.Senha)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarUsuarioExternalCommand, string>
                (item => item.Senha));
    }

    public async Task ValidateDataAsync
    (
        AutenticarUsuarioExternalCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.Senha)
            .Must(item =>
            {
                return !string.IsNullOrEmpty(entity.Senha) &&
                CryptographyHelper.EnhancedVerify
                (
                    request.Senha,
                    entity.Senha
                );
            })
            .WithMessage(MessageHelper
                .AuthenticationFor(request, item => item.Senha));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}