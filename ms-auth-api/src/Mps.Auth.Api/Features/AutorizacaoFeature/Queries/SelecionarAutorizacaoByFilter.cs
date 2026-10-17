using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.AutorizacaoFeature.Queries;

public class SelecionarAutorizacaoByFilterQuery
    : IRequest<IEnumerable<SelecionarAutorizacaoByFilterQueryResponse>>
{
    public string? Tipo { get; set; }
    public string? Valor { get; set; }
}

public class SelecionarAutorizacaoByFilterQueryResponse
{
    public Guid Id { get; set; }
    public string Tipo { get; set; }
    public string Valor { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public bool Perfil { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class SelecionarAutorizacaoByFilterQueryHandler
(
    IRepository<Autorizacao> repository
) : IRequestHandler<SelecionarAutorizacaoByFilterQuery,
    IEnumerable<SelecionarAutorizacaoByFilterQueryResponse>>
{
    private readonly IRepository<Autorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IEnumerable<SelecionarAutorizacaoByFilterQueryResponse>> Handle
    (
        SelecionarAutorizacaoByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        IEnumerable<Autorizacao> entities = await _repository.GetAsync
        (
            request.SearchPredicate(),
            cancellationToken
        );

        return entities.Select(entity => entity.Map<SelecionarAutorizacaoByFilterQueryResponse>());
    }
}

public class SelecionarAutorizacaoByFilterQueryValidator
    : AbstractValidator<SelecionarAutorizacaoByFilterQuery>
{
    public SelecionarAutorizacaoByFilterQueryValidator()
    {
        RuleFor(item => item.Tipo)
            .NotEmpty()
            .When(item => string.IsNullOrEmpty(item.Valor))
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarAutorizacaoByFilterQuery, string?>
                (item => item.Tipo));

        RuleFor(item => item.Valor)
            .NotEmpty()
            .When(item => string.IsNullOrEmpty(item.Tipo))
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarAutorizacaoByFilterQuery, string?>
                (item => item.Valor));
    }
}