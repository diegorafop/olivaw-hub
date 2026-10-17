using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;

public class SelecionarPerfilAcessoAutorizacaoByFilterQuery
    : IRequest<IEnumerable<SelecionarPerfilAcessoAutorizacaoByFilterQueryResponse>>
{
    public Guid? AutorizacaoId { get; set; }
    public Guid? PerfilAcessoId { get; set; }
}

public class SelecionarPerfilAcessoAutorizacaoByFilterQueryResponse
{
    public Guid Id { get; set; }
    public Guid AutorizacaoId { get; set; }
    public Guid? PerfilAcessoId { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class SelecionarPerfilAcessoAutorizacaoByFilterQueryHandler
(
    IRepository<PerfilAcessoAutorizacao> repository
) : IRequestHandler<SelecionarPerfilAcessoAutorizacaoByFilterQuery,
    IEnumerable<SelecionarPerfilAcessoAutorizacaoByFilterQueryResponse>>
{
    private readonly IRepository<PerfilAcessoAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IEnumerable<SelecionarPerfilAcessoAutorizacaoByFilterQueryResponse>> Handle
    (
        SelecionarPerfilAcessoAutorizacaoByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var entities = await GetAsync
        (
            request,
            cancellationToken
        );

        return entities.ToSelecionarPerfilAcessoAutorizacaoByFilterQueryResponse();
    }

    private async Task<IEnumerable<PerfilAcessoAutorizacao>> GetAsync
    (
        SelecionarPerfilAcessoAutorizacaoByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetAsync
        (
            request.SearchPredicate(),
            cancellationToken,
            item => item.PerfilAcesso,
            item => item.Autorizacao
        );
    }
}

public class SelecionarPerfilAcessoAutorizacaoByFilterQueryValidator
    : AbstractValidator<SelecionarPerfilAcessoAutorizacaoByFilterQuery>
{
    public SelecionarPerfilAcessoAutorizacaoByFilterQueryValidator() { }
}