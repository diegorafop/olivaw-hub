using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioFeature.Queries;

// TODO: excluir para usar o SelecionarUsuarioFiltersQuery
public class SelecionarUsuarioByIdManyQuery
    : IRequest<IEnumerable<SelecionarUsuarioByIdManyQueryResponse>>
{
    public IEnumerable<Guid>? IdMany { get; set; }
}

//TODO - retorno fora do padrão, precisa ser "Id", fora do contextos das API's não existe IdExterno
public class SelecionarUsuarioByIdManyQueryResponse
{
    public Guid IdExterno { get; set; }
    public string Login { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public int Matricula { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class SelecionarUsuarioByIdManyQueryHandler
(
    IRepository<UsuarioAutorizacao> repository
) : IRequestHandler<SelecionarUsuarioByIdManyQuery,
    IEnumerable<SelecionarUsuarioByIdManyQueryResponse>>
{
    public readonly IRepository<UsuarioAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IEnumerable<SelecionarUsuarioByIdManyQueryResponse>> Handle
    (
        SelecionarUsuarioByIdManyQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var entities = await GetManyUsuarioAutorizacao
        (
            request,
            cancellationToken
        );

        return entities.ToManySelecionarManyUsuarioByIdManyQueryItemResponse();
    }

    private async Task<IEnumerable<UsuarioAutorizacao>> GetManyUsuarioAutorizacao
    (
        SelecionarUsuarioByIdManyQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetAsync
        (
            SearchPredicate(request),
            cancellationToken,
            item => item.Usuario,
            item => item.Autorizacao
        );
    }

    private static Expression<Func<UsuarioAutorizacao, bool>> SearchPredicate
    (
        SelecionarUsuarioByIdManyQuery request
    )
    {
        Expression<Func<UsuarioAutorizacao, bool>> predicate = item => true;

        if (request.IdMany is not null)
            predicate = predicate.And(item => request
                .IdMany
                .Any(id => id.Equals(item
                    .Usuario
                    .IdExterno)));

        return predicate;
    }
}

public class SelecionarUsuarioByIdManyQueryValidator
    : AbstractValidator<SelecionarUsuarioByIdManyQuery>
{
    public SelecionarUsuarioByIdManyQueryValidator() { }
}