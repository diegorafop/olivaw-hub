using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;

public class AtualizarPerfilAcessoAutorizacaoCommand
    : EntityCommand,
      IRequest<AtualizarPerfilAcessoAutorizacaoCommandResponse>
{
    public Guid Id { get; set; }
    public Guid AutorizacaoId { get; set; }
    public Guid PerfilAcessoId { get; set; }

    public AtualizarPerfilAcessoAutorizacaoCommand() : base() { }
    public AtualizarPerfilAcessoAutorizacaoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AtualizarPerfilAcessoAutorizacaoCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AtualizarPerfilAcessoAutorizacaoCommandHandler
(
    IRepository<PerfilAcessoAutorizacao> repository,
    IRepository<PerfilAcesso> repositoryPerfilAcesso,
    IRepository<Autorizacao> repositoryAutorizacao,
    AtualizarPerfilAcessoAutorizacaoCommandValidator validator
) : IRequestHandler<AtualizarPerfilAcessoAutorizacaoCommand,
    AtualizarPerfilAcessoAutorizacaoCommandResponse>
{
    private readonly IRepository<PerfilAcessoAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepository<PerfilAcesso> _repositoryPerfilAcesso = repositoryPerfilAcesso
        ?? throw new ArgumentNullException(nameof(repositoryPerfilAcesso));

    private readonly IRepository<Autorizacao> _repositoryAutorizacao = repositoryAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryAutorizacao));

    private readonly AtualizarPerfilAcessoAutorizacaoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AtualizarPerfilAcessoAutorizacaoCommandResponse> Handle
    (
        AtualizarPerfilAcessoAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        await ExistsPerfilAcessoAutorizacaoAsync(request, cancellationToken);

        PerfilAcessoAutorizacao entity = await GetFirstAsync(request, cancellationToken);
        PerfilAcesso perfilAcesso = await GetPerfilAcessoAsync(request, cancellationToken);
        Autorizacao autorizacao = await GetAutorizacaoAsync(request, cancellationToken);

        await UpdateAsync(request, entity, perfilAcesso, autorizacao, cancellationToken);

        return entity.Map<AtualizarPerfilAcessoAutorizacaoCommandResponse>();
    }

    private async Task UpdateAsync
    (
        AtualizarPerfilAcessoAutorizacaoCommand request,
        PerfilAcessoAutorizacao entity,
        PerfilAcesso perfilAcesso,
        Autorizacao autorizacao,
        CancellationToken cancellationToken
    )
    {
        request.Update(entity, perfilAcesso, autorizacao);
        await UpdateAsync(entity, request, cancellationToken);
    }

    private async Task<Autorizacao> GetAutorizacaoAsync
    (
        AtualizarPerfilAcessoAutorizacaoCommand request,
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

    private async Task ExistsPerfilAcessoAutorizacaoAsync
    (
        AtualizarPerfilAcessoAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        bool autorizacaoExists = await ExistsAsync
        (
            request,
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            autorizacaoExists,
            cancellationToken
        );
    }

    private async Task<PerfilAcessoAutorizacao> GetFirstAsync
    (
        AtualizarPerfilAcessoAutorizacaoCommand request,
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
        AtualizarPerfilAcessoAutorizacaoCommand request,
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
                    .Autorizacao
                    .IdExterno
                    .Equals(request.AutorizacaoId),
            cancellationToken
        );
    }

    private async Task<PerfilAcesso> GetPerfilAcessoAsync
    (
        AtualizarPerfilAcessoAutorizacaoCommand request,
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
        PerfilAcessoAutorizacao entity,
        AtualizarPerfilAcessoAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AtualizarPerfilAcessoAutorizacaoCommandValidator
    : AbstractValidator<AtualizarPerfilAcessoAutorizacaoCommand>
{
    public AtualizarPerfilAcessoAutorizacaoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarPerfilAcessoAutorizacaoCommand, Guid>
                (item => item.Id));

        RuleFor(item => item.PerfilAcessoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarPerfilAcessoAutorizacaoCommand, Guid>
                (item => item.PerfilAcessoId));

        RuleFor(item => item.AutorizacaoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarPerfilAcessoAutorizacaoCommand, Guid>
                (item => item.AutorizacaoId));
    }

    public async Task ValidateDataAsync
    (
        AtualizarPerfilAcessoAutorizacaoCommand request,
        bool nomeGrupoExists,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.AutorizacaoId)
            .Must(item => { return !nomeGrupoExists; })
            .WithMessage(MessageHelper
                .DuplicateAutenticacao(request, item => item.AutorizacaoId));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}