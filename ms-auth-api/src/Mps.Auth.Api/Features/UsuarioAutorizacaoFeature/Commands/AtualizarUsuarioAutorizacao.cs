using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Commands;

public class AtualizarUsuarioAutorizacaoCommand
    : EntityCommand,
      IRequest<AtualizarUsuarioAutorizacaoCommandResponse>
{
    public Guid Id { get; set; }
    public Guid AutorizacaoId { get; set; }

    public AtualizarUsuarioAutorizacaoCommand() : base() { }
    public AtualizarUsuarioAutorizacaoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AtualizarUsuarioAutorizacaoCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AtualizarUsuarioAutorizacaoCommandHandler
(
    IRepository<UsuarioAutorizacao> repository,
    IRepository<Autorizacao> repositoryAutorizacao,
    AtualizarUsuarioAutorizacaoCommandValidator validator
) : IRequestHandler<AtualizarUsuarioAutorizacaoCommand,
    AtualizarUsuarioAutorizacaoCommandResponse>
{
    private readonly IRepository<UsuarioAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepository<Autorizacao> _repositoryAutorizacao = repositoryAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryAutorizacao));

    private readonly AtualizarUsuarioAutorizacaoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AtualizarUsuarioAutorizacaoCommandResponse> Handle
    (
        AtualizarUsuarioAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        UsuarioAutorizacao entity = await GetFirstAsync(request, cancellationToken);
        Autorizacao autorizacao = await GetAsync(request, entity.UsuarioId, cancellationToken);
        await UpdateAsync(request, entity, autorizacao, cancellationToken);

        return entity.Map<AtualizarUsuarioAutorizacaoCommandResponse>();
    }

    private async Task<UsuarioAutorizacao> GetFirstAsync
    (
        AtualizarUsuarioAutorizacaoCommand request,
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

    private async Task UpdateAsync
    (
        AtualizarUsuarioAutorizacaoCommand request,
        UsuarioAutorizacao entity,
        Autorizacao autorizacao,
        CancellationToken cancellationToken
    )
    {
        entity.Update(autorizacao, request.UsuarioLogadoId);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Autorizacao> GetAsync
    (
        AtualizarUsuarioAutorizacaoCommand request,
        int usuarioId,
        CancellationToken cancellationToken
    )
    {
        Autorizacao autorizacao = await GetAutorizacaoFirstAsync
        (
            request,
            cancellationToken
        );

        bool exists = await _repository.ExistsAsync
        (
            SearchPredicate(usuarioId, autorizacao),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            exists,
            cancellationToken
        );

        return autorizacao;
    }

    private async Task<Autorizacao> GetAutorizacaoFirstAsync
    (
        AtualizarUsuarioAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repositoryAutorizacao.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.AutorizacaoId),
            cancellationToken
        );
    }

    private static Expression<Func<UsuarioAutorizacao, bool>> SearchPredicate
    (
        int usuarioId,
        Autorizacao autorizacao
    )
    {
        return item => item
            .Usuario
            .Id
            .Equals(usuarioId)
            && item
                .Autorizacao
                .Id
                .Equals(autorizacao.Id);
    }
}

public class AtualizarUsuarioAutorizacaoCommandValidator
    : AbstractValidator<AtualizarUsuarioAutorizacaoCommand>
{
    public AtualizarUsuarioAutorizacaoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarUsuarioAutorizacaoCommand, Guid>
                (item => item.Id));

        RuleFor(item => item.AutorizacaoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarUsuarioAutorizacaoCommand, Guid>
                (item => item.AutorizacaoId));
    }

    public async Task ValidateDataAsync
    (
        AtualizarUsuarioAutorizacaoCommand request,
        bool exists,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.AutorizacaoId)
            .Must(item => { return !exists; })
            .WithMessage(MessageHelper
                .DuplicateKeyFor(request, item => item.AutorizacaoId));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}