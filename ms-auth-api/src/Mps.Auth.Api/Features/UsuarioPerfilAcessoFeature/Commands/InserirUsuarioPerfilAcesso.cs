using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Commands;

public class InserirUsuarioPerfilAcessoCommand
    : EntityCommand,
      IRequest<InserirUsuarioPerfilAcessoCommandResponse>
{
    public Guid UsuarioId { get; set; }
    public Guid PerfilAcessoId { get; set; }

    public InserirUsuarioPerfilAcessoCommand() : base() { }
    public InserirUsuarioPerfilAcessoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class InserirUsuarioPerfilAcessoCommandResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class InserirUsuarioPerfilAcessoCommandHandler
(
    IRepository<UsuarioPerfilAcesso> repository,
    IRepository<PerfilAcesso> repositoryPerfilAcesso,
    IRepository<Usuario> repositoryUsuario,
    InserirUsuarioPerfilAcessoCommandValidator validator
) : IRequestHandler<InserirUsuarioPerfilAcessoCommand,
    InserirUsuarioPerfilAcessoCommandResponse>
{
    private readonly IRepository<UsuarioPerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepository<PerfilAcesso> _repositoryPerfilAcesso = repositoryPerfilAcesso
        ?? throw new ArgumentNullException(nameof(repositoryPerfilAcesso));

    private readonly IRepository<Usuario> _repositoryUsuario = repositoryUsuario
        ?? throw new ArgumentNullException(nameof(repositoryUsuario));

    private readonly InserirUsuarioPerfilAcessoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<InserirUsuarioPerfilAcessoCommandResponse> Handle
    (
        InserirUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        await ExistsUsuarioPerfilAcessoAsync(request, cancellationToken);

        PerfilAcesso perfilAcesso = await GetPerfilAcessoAsync(request, cancellationToken);
        Usuario usuario = await GetUsuarioAsync(request, cancellationToken);
        UsuarioPerfilAcesso entity = request.ToDomain(perfilAcesso, usuario);
        await AddAsync(entity, cancellationToken);

        return entity.Map<InserirUsuarioPerfilAcessoCommandResponse>();
    }

    private async Task ExistsUsuarioPerfilAcessoAsync
    (
        InserirUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        bool perfilAcessoExists = await ExistsAsync
        (
            request,
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            perfilAcessoExists,
            cancellationToken
        );
    }

    private async Task<bool> ExistsAsync
    (
        InserirUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.ExistsAsync
        (
            item => item
                .PerfilAcesso
                .IdExterno
                .Equals(request.PerfilAcessoId)
                && item
                    .Usuario
                    .IdExterno
                    .Equals(request.UsuarioId)
            , cancellationToken
        );
    }

    private async Task<Usuario> GetUsuarioAsync
    (
        InserirUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repositoryUsuario.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.UsuarioId),
            cancellationToken
        );
    }

    private async Task<PerfilAcesso> GetPerfilAcessoAsync
    (
        InserirUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repositoryPerfilAcesso.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.PerfilAcessoId),
            cancellationToken
        );
    }

    private async Task AddAsync
    (
        Domain.UsuarioPerfilAcesso entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class InserirUsuarioPerfilAcessoCommandValidator
    : AbstractValidator<InserirUsuarioPerfilAcessoCommand>
{
    public InserirUsuarioPerfilAcessoCommandValidator()
    {
        RuleFor(item => item.PerfilAcessoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioPerfilAcessoCommand, Guid>
                (item => item.PerfilAcessoId));

        RuleFor(item => item.UsuarioId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioPerfilAcessoCommand, Guid>
                (item => item.UsuarioId));
    }

    public async Task ValidateDataAsync
    (
        InserirUsuarioPerfilAcessoCommand request,
        bool nomeGrupoExists,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.PerfilAcessoId)
            .Must(item => { return !nomeGrupoExists; })
            .WithMessage(MessageHelper
                .DuplicateAutenticacao(request, item => item.PerfilAcessoId));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}