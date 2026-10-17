using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;

public class SelecionarPerfilAcessoAutorizacaoByIdQuery
    : IRequest<SelecionarPerfilAcessoAutorizacaoByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class SelecionarPerfilAcessoAutorizacaoByIdQueryResponse
{
    public Guid AutorizacaoId { get; set; }
    public Guid PerfilAcessoId { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class SelecionarPerfilAcessoAutorizacaoByIdQueryHandler
(
    IRepository<PerfilAcessoAutorizacao> repository,
    SelecionarPerfilAcessoAutorizacaoByIdQueryValidator validator
) : IRequestHandler<SelecionarPerfilAcessoAutorizacaoByIdQuery,
    SelecionarPerfilAcessoAutorizacaoByIdQueryResponse>
{
    private readonly IRepository<PerfilAcessoAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly SelecionarPerfilAcessoAutorizacaoByIdQueryValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<SelecionarPerfilAcessoAutorizacaoByIdQueryResponse> Handle
    (
        SelecionarPerfilAcessoAutorizacaoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        PerfilAcessoAutorizacao entity = await GetFirstAsync(request, cancellationToken);

        return entity.ToSelecionarPerfilAcessoAutorizacaoByIdQueryResponse();
    }

    private async Task<PerfilAcessoAutorizacao> GetFirstAsync
    (
        SelecionarPerfilAcessoAutorizacaoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var entity = await _repository.GetFirstAsync
        (
            SearchPredicate(request),
            cancellationToken,
            item => item.PerfilAcesso,
            item => item.Autorizacao
        );

        await _validator.ValidateDataAsync
        (
            request,
            entity,
            cancellationToken
        );

        return entity;
    }

    private static Expression<Func<PerfilAcessoAutorizacao, bool>>
        SearchPredicate
    (
        SelecionarPerfilAcessoAutorizacaoByIdQuery request
    )
    {
        return item => item
            .IdExterno
            .Equals(request.Id);
    }
}

public class SelecionarPerfilAcessoAutorizacaoByIdQueryValidator
    : AbstractValidator<SelecionarPerfilAcessoAutorizacaoByIdQuery>
{
    public SelecionarPerfilAcessoAutorizacaoByIdQueryValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarPerfilAcessoAutorizacaoByIdQuery, Guid>
                (item => item.Id));
    }

    public async Task ValidateDataAsync
    (
        SelecionarPerfilAcessoAutorizacaoByIdQuery request,
        PerfilAcessoAutorizacao entity,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.Id)
            .Must(item => { return entity != null; })
            .WithMessage(MessageHelper
                .NotFoundFor(request, item => item.Id));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}