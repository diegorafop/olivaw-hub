using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Extensions.Validations;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioFeature.Internals;

public class InserirUsuarioInternalCommand
    : EntityCommand,
      IRequest<InserirUsuarioInternalCommandResponse>
{
    public Guid? Id { get; set; }
    public string Login { get; set; }
    public string? Senha { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string? IntegracaoId { get; set; }
    public StatusIntegracaoEnum StatusIntegracaoEnum { get; set; }

    public InserirUsuarioInternalCommand() { }
    public InserirUsuarioInternalCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class InserirUsuarioInternalCommandResponse
{
    public Usuario Usuario { get; set; }
}

public class InserirUsuarioInternalCommandHandler
(
    IRepositoryUsuario repository,
    IRepositoryPerfilAcesso repositoryPerfilAcesso,
    IRepository<UsuarioPerfilAcesso> repositoryUsuarioPerfilAcesso,
    IConfiguration configuration,
    InserirUsuarioInternalCommandValidator validator
) : IRequestHandler<InserirUsuarioInternalCommand,
    InserirUsuarioInternalCommandResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepositoryPerfilAcesso _repositoryPerfilAcesso = repositoryPerfilAcesso
        ?? throw new ArgumentNullException(nameof(repositoryPerfilAcesso));

    private readonly IRepository<UsuarioPerfilAcesso> _repositoryUsuarioPerfilAcesso = repositoryUsuarioPerfilAcesso
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioPerfilAcesso));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    private readonly InserirUsuarioInternalCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<InserirUsuarioInternalCommandResponse> Handle
    (
        InserirUsuarioInternalCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        if (request.StatusIntegracaoEnum.Equals(StatusIntegracaoEnum.None))
            request.StatusIntegracaoEnum = StatusIntegracaoEnum.NaoIntegrar;

        await ExistsAsync(request, cancellationToken);

        var entity = request.ToDomain();

        await _repository.AddAsync(entity, cancellationToken);

        var usuarioPerfilAcesso = await GetUsuarioPerfilAcessoBasicoAsync
        (
            request,
            entity,
            cancellationToken
        );

        await _repositoryUsuarioPerfilAcesso.AddAsync
        (
            usuarioPerfilAcesso,
            cancellationToken
        );

        entity.Perfils = [usuarioPerfilAcesso];

        return new InserirUsuarioInternalCommandResponse()
        {
            Usuario = entity
        };
    }

    private async Task<UsuarioPerfilAcesso> GetUsuarioPerfilAcessoBasicoAsync
    (
        InserirUsuarioInternalCommand request,
        Usuario usuario,
        CancellationToken cancellationToken
    )
    {
        return request.ToDomain
        (
            usuario,
            await GetPerfilAcessoBasicoAsync(cancellationToken)
        );
    }

    private async Task<PerfilAcesso> GetPerfilAcessoBasicoAsync
    (
        CancellationToken cancellationToken
    )
    {
        string perfilAcessoBasicoNome = _configuration.GetValue<string>
        (
            "constants:perfil-acesso:basico:nome"
        );

        return await _repositoryPerfilAcesso.GetFirstAsync
        (
            item => item
                .Nome
                .Equals(perfilAcessoBasicoNome),
            cancellationToken,
            item => item.Autorizacaos
        );
    }

    private async Task ExistsAsync
    (
        InserirUsuarioInternalCommand request,
        CancellationToken cancellationToken
    )
    {
        var entity = await _repository.GetFirstOrDefaultAsync
        (
            SearchPredicate(request),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            entity,
            cancellationToken
        );
    }

    private static Expression<Func<Usuario, bool>> SearchPredicate
    (
        InserirUsuarioInternalCommand request
    )
    {
        Expression<Func<Usuario, bool>> expression = item =>
            item.Login.ToUpper() == request.Login.ToUpper() ||
            item.Email.ToUpper() == request.Email.ToUpper();

        if (!string.IsNullOrEmpty(request.IntegracaoId))
        {
            expression = expression.Or(item => item.IntegracaoId == request.IntegracaoId);
        }

        return expression;
    }
}

public class InserirUsuarioInternalCommandValidator
    : AbstractValidator<InserirUsuarioInternalCommand>
{
    public InserirUsuarioInternalCommandValidator()
    {
        this.NotEmpty(item => item.Email);
        this.NotEmpty(item => item.Email);
        this.NotEmpty(item => item.UsuarioLogadoId);
    }

    public async Task ValidateDataAsync
    (
        InserirUsuarioInternalCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        this
            .DuplicateKeyFor
            (
                item => item.Login,
                request,
                item => !entity
                    .Login
                    .ToUpper()
                    .Equals(request
                        .Login
                        .ToUpper())
            )
            .When(item => entity is not null);

        this
            .DuplicateKeyFor
            (
                item => item.Email,
                request,
                item => !entity
                    .Email
                    .ToUpper()
                    .Equals(request
                        .Email
                        .ToUpper())
            )
            .When(item => entity is not null);

        this
            .DuplicateKeyFor
            (
                item => item.IntegracaoId,
                request,
                item => !entity
                    .IntegracaoId
                    !.ToUpper()
                    .Equals(request
                        .IntegracaoId
                        !.ToUpper())
            )
            .When(item =>
                !string.IsNullOrEmpty(entity?.IntegracaoId) &&
                !string.IsNullOrEmpty(item.IntegracaoId)
            );

        await this.ValidateAndThrowAsync(request, cancellationToken);
    }
}