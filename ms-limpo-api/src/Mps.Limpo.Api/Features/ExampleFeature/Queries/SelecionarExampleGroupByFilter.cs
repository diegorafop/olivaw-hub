using FluentValidation;
using MediatR;
using Mps.Limpo.Api.Extensions.Entities;
using Mps.OlivawCore.Api.Extensions.Entities;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Domain;
using Mps.OlivawCore.Repositories;

namespace Mps.Limpo.Api.Features.ExampleFeature.Queries;

public class SelecionarExampleByFilterQuery
    : IRequest<IEnumerable<SelecionarExampleByFilterQueryResponse>>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Permission { get; set; }
}

public class SelecionarExampleByFilterQueryResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
    public Guid UsuarioLogadoId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public bool Permission { get; set; }
}

public class SelecionarExampleByFilterQueryHandler
    : IRequestHandler<SelecionarExampleByFilterQuery,
      IEnumerable<SelecionarExampleByFilterQueryResponse>>
{
    private readonly IRepository<Example> _repository;

    public SelecionarExampleByFilterQueryHandler
    (
        IRepository<Example> repository
    )
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<SelecionarExampleByFilterQueryResponse>> Handle
    (
        SelecionarExampleByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        IEnumerable<Example> entities = await GetAsync
        (
            request,
            cancellationToken
        );

        return entities.Select(item => item.Map<SelecionarExampleByFilterQueryResponse>());
    }

    private async Task<IEnumerable<Example>> GetAsync
    (
        SelecionarExampleByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetAsync
        (
            request.SearchPredicate(),
            cancellationToken
        );
    }

    public class SelecionarExampleByFilterQueryValidator
        : AbstractValidator<SelecionarExampleByFilterQuery>
    {
        public SelecionarExampleByFilterQueryValidator() { }
    }
}