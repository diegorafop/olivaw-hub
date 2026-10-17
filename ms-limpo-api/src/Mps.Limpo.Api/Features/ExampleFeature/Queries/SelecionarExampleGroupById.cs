using FluentValidation;
using MediatR;
using Mps.Limpo.Api.Extensions.Entities;
using Mps.OlivawCore.Api.Extensions.Entities;
using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Domain;
using Mps.OlivawCore.Repositories;

namespace Mps.Limpo.Api.Features.ExampleFeature.Queries;

public class SelecionarExampleByIdQuery
    : IRequest<SelecionarExampleByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class SelecionarExampleByIdQueryResponse
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

public class SelecionarExampleByIdQueryHandler
(
    IRepository<Example> repository
) : IRequestHandler<SelecionarExampleByIdQuery,
    SelecionarExampleByIdQueryResponse>
{
    private readonly IRepository<Example> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<SelecionarExampleByIdQueryResponse> Handle
    (
        SelecionarExampleByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Example entity = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        return entity.Map<SelecionarExampleByIdQueryResponse>();
    }

    private async Task<Example> GetFirstAsync
    (
        SelecionarExampleByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            request.SearchPredicate(),
            cancellationToken
        );
    }
}

public class SelecionarExampleByIdQueryValidator
    : AbstractValidator<SelecionarExampleByIdQuery>
{
    public SelecionarExampleByIdQueryValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarExampleByIdQuery, Guid>(item => item.Id));
    }
}