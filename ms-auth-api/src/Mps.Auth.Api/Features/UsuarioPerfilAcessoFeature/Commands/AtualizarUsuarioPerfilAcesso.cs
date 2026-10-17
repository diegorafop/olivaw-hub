using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Commands;

public class AtualizarUsuarioPerfilAcessoCommand
    : EntityCommand,
      IRequest<AtualizarUsuarioPerfilAcessoCommandResponse>
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid PerfilAcessoId { get; set; }

    public AtualizarUsuarioPerfilAcessoCommand() : base() { }
    public AtualizarUsuarioPerfilAcessoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AtualizarUsuarioPerfilAcessoCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AtualizarUsuarioPerfilAcessoCommandHandler(
    IRepository<UsuarioPerfilAcesso> repository,
    IRepository<PerfilAcesso> repositoryPerfilAcesso,
    IRepository<Usuario> repositoryUsuario,
    AtualizarUsuarioPerfilAcessoCommandValidator validator
    )
        : IRequestHandler<AtualizarUsuarioPerfilAcessoCommand,
      AtualizarUsuarioPerfilAcessoCommandResponse>
{
    private readonly IRepository<UsuarioPerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepository<PerfilAcesso> _repositoryPerfilAcesso = repositoryPerfilAcesso
        ?? throw new ArgumentNullException(nameof(repositoryPerfilAcesso));

    private readonly IRepository<Usuario> _repositoryUsuario = repositoryUsuario
        ?? throw new ArgumentNullException(nameof(repositoryUsuario));

    private readonly AtualizarUsuarioPerfilAcessoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AtualizarUsuarioPerfilAcessoCommandResponse> Handle
    (
        AtualizarUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        await ExistsUsuarioPerfilAcessoAsync(request, cancellationToken);

        UsuarioPerfilAcesso entity = await GetFirstAsync(request, cancellationToken);
        PerfilAcesso perfilAcesso = await GetPerfilAcessoAsync(request, cancellationToken);
        Usuario usuario = await GetUsuarioAsync(request, cancellationToken);

        await UpdateAsync(request, entity, perfilAcesso, usuario, cancellationToken);

        return entity.Map<AtualizarUsuarioPerfilAcessoCommandResponse>();
    }

    private async Task UpdateAsync
    (
        AtualizarUsuarioPerfilAcessoCommand request,
        Domain.UsuarioPerfilAcesso entity,
        PerfilAcesso perfilAcesso,
        Usuario usuario,
        CancellationToken cancellationToken
    )
    {
        request.Update(entity, perfilAcesso, usuario);
        await UpdateAsync(entity, cancellationToken);
    }

    private async Task<Usuario> GetUsuarioAsync
    (
        AtualizarUsuarioPerfilAcessoCommand request,
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

    private async Task ExistsUsuarioPerfilAcessoAsync
    (
        AtualizarUsuarioPerfilAcessoCommand request,
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

    private async Task<Domain.UsuarioPerfilAcesso> GetFirstAsync
    (
        AtualizarUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id),
            cancellationToken
        );
    }

    private async Task<bool> ExistsAsync
    (
        AtualizarUsuarioPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.ExistsAsync
        (
            SearchPredicate(request),
            cancellationToken
        );
    }

    private static Expression<Func<UsuarioPerfilAcesso, bool>> SearchPredicate
    (
        AtualizarUsuarioPerfilAcessoCommand request
    )
    {
        return item => item
            .PerfilAcesso
            .IdExterno
            .Equals(request.PerfilAcessoId)
            && item
                .Usuario
                .IdExterno
                .Equals(request.UsuarioId);
    }

    private async Task<PerfilAcesso> GetPerfilAcessoAsync
    (
        AtualizarUsuarioPerfilAcessoCommand request,
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

    private async Task UpdateAsync
    (
        UsuarioPerfilAcesso entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AtualizarUsuarioPerfilAcessoCommandValidator
    : AbstractValidator<AtualizarUsuarioPerfilAcessoCommand>
{
    public AtualizarUsuarioPerfilAcessoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarUsuarioPerfilAcessoCommand, Guid>
                (item => item.Id));

        RuleFor(item => item.PerfilAcessoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarUsuarioPerfilAcessoCommand, Guid>
                (item => item.PerfilAcessoId));

        RuleFor(item => item.UsuarioId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarUsuarioPerfilAcessoCommand, Guid>
                (item => item.UsuarioId));
    }

    public async Task ValidateDataAsync
    (
        AtualizarUsuarioPerfilAcessoCommand request,
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