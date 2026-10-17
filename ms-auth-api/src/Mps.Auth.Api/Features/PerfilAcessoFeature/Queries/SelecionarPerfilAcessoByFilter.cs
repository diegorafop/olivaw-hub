using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.PerfilAcessoFeature.Queries;

public class SelecionarPerfilAcessoByFilterQuery
    : IRequest<IEnumerable<SelecionarPerfilAcessoByFilterQueryResponse>>
{
    public string? Nome { get; set; }
}

public class SelecionarPerfilAcessoByFilterQueryResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class SelecionarPerfilAcessoByFilterQueryHandler
(
    IRepository<PerfilAcesso> repository
) : IRequestHandler<SelecionarPerfilAcessoByFilterQuery,
    IEnumerable<SelecionarPerfilAcessoByFilterQueryResponse>>
{
    public readonly IRepository<PerfilAcesso> _repository = repository;

    public async Task<IEnumerable<SelecionarPerfilAcessoByFilterQueryResponse>> Handle
    (
        SelecionarPerfilAcessoByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        IEnumerable<PerfilAcesso> entities = await GetAsync(request, cancellationToken);

        return entities.Select(entity => entity.Map<SelecionarPerfilAcessoByFilterQueryResponse>());
    }

    private async Task<IEnumerable<PerfilAcesso>> GetAsync
    (
        SelecionarPerfilAcessoByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetAsync
        (
            request.SearchPredicate(),
            cancellationToken,
            item => item.Autorizacaos
        );
    }
}

public class SelecionarPerfilAcessoByFilterQueryValidator
    : AbstractValidator<SelecionarPerfilAcessoByFilterQuery>
{
    public SelecionarPerfilAcessoByFilterQueryValidator() { }
}