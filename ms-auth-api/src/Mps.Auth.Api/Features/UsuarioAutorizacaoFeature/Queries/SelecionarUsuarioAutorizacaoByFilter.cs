using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Queries;

public class SelecionarUsuarioAutorizacaoByFilterQuery
    : IRequest<IEnumerable<SelecionarUsuarioAutorizacaoByFilterQueryResponse>>
{
    public Guid AutorizacaoId { get; set; }
    public Guid UsuarioId { get; set; }
}

public class SelecionarUsuarioAutorizacaoByFilterQueryResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid AutorizacaoId { get; set; }
}

public class SelecionarUsuarioAutorizacaoByFilterQueryHandler
(
    IRepository<UsuarioAutorizacao> repository
) : IRequestHandler<SelecionarUsuarioAutorizacaoByFilterQuery,
    IEnumerable<SelecionarUsuarioAutorizacaoByFilterQueryResponse>>
{
    public readonly IRepository<UsuarioAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IEnumerable<SelecionarUsuarioAutorizacaoByFilterQueryResponse>> Handle
    (
        SelecionarUsuarioAutorizacaoByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        IEnumerable<UsuarioAutorizacao> entities = await GetAsync
        (
            request,
            cancellationToken
        );

        return entities.Select(entity => entity.ToSelecionarUsuarioAutorizacaoByFilterQueryResponse());
    }

    private async Task<IEnumerable<UsuarioAutorizacao>> GetAsync
    (
        SelecionarUsuarioAutorizacaoByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetAsync
        (
            SearchPredicate(request),
            cancellationToken,
            item => item.Autorizacao,
            item => item.Usuario
        );
    }

    private static Expression<Func<UsuarioAutorizacao, bool>> SearchPredicate
    (
        SelecionarUsuarioAutorizacaoByFilterQuery request
    )
    {
        Expression<Func<UsuarioAutorizacao, bool>> predicate = a => true;

        if (!request.UsuarioId.Equals(Guid.Empty))
            predicate = predicate.And(item => item
                .Usuario
                .IdExterno
                .Equals(request.UsuarioId));

        if (!request.AutorizacaoId.Equals(Guid.Empty))
            predicate = predicate.And(item => item
                .Autorizacao
                .IdExterno
                .Equals(request.AutorizacaoId));

        return predicate;
    }
}

public class SelecionarUsuarioAutorizacaoByFilterQueryValidator
    : AbstractValidator<SelecionarUsuarioAutorizacaoByFilterQuery>
{
    public SelecionarUsuarioAutorizacaoByFilterQueryValidator()
    {
        RuleFor(item => item.UsuarioId)
            .NotEmpty()
            .When(item => item.AutorizacaoId.Equals(Guid.Empty))
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarUsuarioAutorizacaoByFilterQuery, Guid>
                (item => item.UsuarioId));

        RuleFor(item => item.AutorizacaoId)
            .NotEmpty()
            .When(item => item.UsuarioId.Equals(Guid.Empty))
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarUsuarioAutorizacaoByFilterQuery, Guid>
                (item => item.AutorizacaoId));
    }
}