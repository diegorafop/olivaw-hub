using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;

public class InserirPerfilAcessoAutorizacaoCommand
    : EntityCommand,
      IRequest<InserirPerfilAcessoAutorizacaoCommandResponse>
{
    public Guid AutorizacaoId { get; set; }
    public Guid PerfilAcessoId { get; set; }

    public InserirPerfilAcessoAutorizacaoCommand() : base() { }
    public InserirPerfilAcessoAutorizacaoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class InserirPerfilAcessoAutorizacaoCommandResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class InserirPerfilAcessoAutorizacaoCommandHandler
(
    IRepository<PerfilAcessoAutorizacao> repository,
    IRepository<PerfilAcesso> repositoryPerfilAcesso,
    IRepository<Autorizacao> repositoryAutorizacao,
    InserirPerfilAcessoAutorizacaoCommandValidator validator
) : IRequestHandler<InserirPerfilAcessoAutorizacaoCommand,
    InserirPerfilAcessoAutorizacaoCommandResponse>
{
    private readonly IRepository<PerfilAcessoAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepository<PerfilAcesso> _repositoryPerfilAcesso = repositoryPerfilAcesso
        ?? throw new ArgumentNullException(nameof(repositoryPerfilAcesso));

    private readonly IRepository<Autorizacao> _repositoryAutorizacao = repositoryAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryAutorizacao));

    private readonly InserirPerfilAcessoAutorizacaoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<InserirPerfilAcessoAutorizacaoCommandResponse> Handle
    (
        InserirPerfilAcessoAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        await ExistsPerfilAcessoAutorizacaoAsync(request, cancellationToken);

        PerfilAcesso perfilAcesso = await GetPerfilAcessoAsync(request, cancellationToken);
        Autorizacao autorizacao = await GetAutorizacaoAsync(request, cancellationToken);
        PerfilAcessoAutorizacao entity = request.ToDomain(perfilAcesso, autorizacao);
        await AddAsync(entity, cancellationToken);

        return entity.Map<InserirPerfilAcessoAutorizacaoCommandResponse>();
    }

    private async Task ExistsPerfilAcessoAutorizacaoAsync
    (
        InserirPerfilAcessoAutorizacaoCommand request,
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

    private async Task<bool> ExistsAsync
    (
        InserirPerfilAcessoAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.ExistsAsync
        (
            SearchPredicate(request),
            cancellationToken
        );
    }

    private static Expression<Func<PerfilAcessoAutorizacao, bool>> SearchPredicate
    (
        InserirPerfilAcessoAutorizacaoCommand request
    )
    {
        return item => item
            .PerfilAcesso
            .IdExterno
            .Equals(request.PerfilAcessoId)
            && item
                .Autorizacao
                .IdExterno
                .Equals(request.AutorizacaoId);
    }

    private async Task<Autorizacao> GetAutorizacaoAsync
    (
        InserirPerfilAcessoAutorizacaoCommand request,
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

    private async Task<PerfilAcesso> GetPerfilAcessoAsync
    (
        InserirPerfilAcessoAutorizacaoCommand request,
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
        PerfilAcessoAutorizacao entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class InserirPerfilAcessoAutorizacaoCommandValidator
    : AbstractValidator<InserirPerfilAcessoAutorizacaoCommand>
{
    public InserirPerfilAcessoAutorizacaoCommandValidator()
    {
        RuleFor(item => item.PerfilAcessoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirPerfilAcessoAutorizacaoCommand, Guid>
                (item => item.PerfilAcessoId));

        RuleFor(item => item.AutorizacaoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirPerfilAcessoAutorizacaoCommand, Guid>
                (item => item.AutorizacaoId));
    }

    public async Task ValidateDataAsync
    (
        InserirPerfilAcessoAutorizacaoCommand request,
        bool nomeGrupoExists,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.AutorizacaoId)
            .Must(item => { return !nomeGrupoExists; })
            .WithMessage(MessageHelper
                .DuplicateAutenticacao
                (
                    request,
                    item => item.AutorizacaoId)
                );

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}