using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.PerfilAcessoFeature.Queries;

public class SelecionarPerfilAcessoByIdQuery
    : IRequest<SelecionarPerfilAcessoByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class SelecionarPerfilAcessoByIdQueryResponse
{
    public string Nome { get; set; }
    public string? Descricao { get; set; }
    public IEnumerable<Autorizacao> Autorizacaos { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class SelecionarPerfilAcessoByIdQueryHandler
(
    IRepository<PerfilAcesso> repository
) : IRequestHandler<SelecionarPerfilAcessoByIdQuery,
    SelecionarPerfilAcessoByIdQueryResponse>
{
    public readonly IRepository<PerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<SelecionarPerfilAcessoByIdQueryResponse> Handle
    (
        SelecionarPerfilAcessoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        PerfilAcesso entity = await GetFirstAsync(request, cancellationToken);

        return entity.Map<SelecionarPerfilAcessoByIdQueryResponse>();
    }

    private async Task<PerfilAcesso> GetFirstAsync
    (
        SelecionarPerfilAcessoByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            SearchPredicate(request),
            cancellationToken,
            item => item.Autorizacaos
        );
    }

    private static Expression<Func<PerfilAcesso, bool>>
        SearchPredicate
    (
        SelecionarPerfilAcessoByIdQuery request
    )
    {
        return item => item
            .IdExterno
            .Equals(request.Id);
    }
}

public class SelecionarPerfilAcessoByIdQueryValidator
    : AbstractValidator<SelecionarPerfilAcessoByIdQuery>
{
    public SelecionarPerfilAcessoByIdQueryValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<SelecionarPerfilAcessoByIdQuery, Guid>
                (item => item.Id));
    }
}