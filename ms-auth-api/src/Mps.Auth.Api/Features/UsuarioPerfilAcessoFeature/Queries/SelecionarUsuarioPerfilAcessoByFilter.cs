using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Queries;

public class SelecionarUsuarioPerfilAcessoByFilterQuery
    : IRequest<IEnumerable<SelecionarUsuarioPerfilAcessoByFilterQueryResponse>>
{
    public Guid? UsuarioId { get; set; }
    public Guid? PerfilAcessoId { get; set; }
}

public class SelecionarUsuarioPerfilAcessoByFilterQueryResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? PerfilAcessoId { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class SelecionarUsuarioPerfilAcessoByFilterQueryHandler
(
    IRepository<Domain.UsuarioPerfilAcesso> repository
) : IRequestHandler<SelecionarUsuarioPerfilAcessoByFilterQuery,
    IEnumerable<SelecionarUsuarioPerfilAcessoByFilterQueryResponse>>
{
    private readonly IRepository<Domain.UsuarioPerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IEnumerable<SelecionarUsuarioPerfilAcessoByFilterQueryResponse>> Handle
    (
        SelecionarUsuarioPerfilAcessoByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var entities = await _repository.GetAsync
        (
            request.SearchPredicate(),
            cancellationToken,
            item => item.PerfilAcesso,
            item => item.Usuario
        );

        return entities.ToSelecionarUsuarioPerfilAcessoByFilterQueryResponse();
    }
}

public class SelecionarUsuarioPerfilAcessoByFilterQueryValidator
    : AbstractValidator<SelecionarUsuarioPerfilAcessoByFilterQuery>
{
    public SelecionarUsuarioPerfilAcessoByFilterQueryValidator() { }
}