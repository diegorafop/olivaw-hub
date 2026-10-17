using MediatR;

using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Helpers;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioFeature.Queries;

public class VerificarExistenciaUsuarioQuery
    : EntityCommand,
      IRequest<VerificarExistenciaUsuarioQueryResponse>
{
    public Guid? Id { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
}

public class VerificarExistenciaUsuarioQueryResponse
{
    public bool? ExisteId { get; set; }
    public bool? ExisteLogin { get; set; }
    public bool? ExisteEmail { get; set; }
}

public class VerificarExistenciaUsuarioCommandHandler
(
    IRepositoryUsuario repository
) : IRequestHandler<VerificarExistenciaUsuarioQuery,
    VerificarExistenciaUsuarioQueryResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<VerificarExistenciaUsuarioQueryResponse> Handle
    (
        VerificarExistenciaUsuarioQuery request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var entities = await GetAsync(request, cancellationToken);

        var existeId = ExisteUsuarioComPropriedade
        (
            request,
            SearchPredicateId,
            entities
        );

        var existeEmail = ExisteUsuarioComPropriedade
        (
            request,
            SearchPredicateEmail,
            entities
        );

        var existeLogin = ExisteUsuarioComPropriedade
        (
            request,
            SearchPredicateLogin,
            entities
        );

        return new()
        {
            ExisteId = existeId,
            ExisteLogin = existeLogin,
            ExisteEmail = existeEmail
        };
    }

    private static bool? ExisteUsuarioComPropriedade
    (
        VerificarExistenciaUsuarioQuery request,
        Func<VerificarExistenciaUsuarioQuery, Expression<Func<Usuario, bool>>?> predicateFunc,
        IEnumerable<Usuario> entities
    )
    {
        var expressionId = predicateFunc(request);

        if (expressionId is not null)
            return entities.Any(expressionId.Compile());

        return null;
    }

    private async Task<IEnumerable<Usuario>> GetAsync
    (
        VerificarExistenciaUsuarioQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetAsync
        (
            SearchPredicate(request),
            cancellationToken
        );
    }

    private static Expression<Func<Usuario, bool>>?
        SearchPredicateId
    (
        VerificarExistenciaUsuarioQuery request
    )
    {
        if (request.Id.HasValue)
            return item => item
                .IdExterno
                .Equals(request.Id.Value);

        return null;
    }

    private static Expression<Func<Usuario, bool>>?
        SearchPredicateLogin
    (
        VerificarExistenciaUsuarioQuery request
    )
    {
        if (!string.IsNullOrEmpty(request.Login))
            return item => item
                .Login
                .Equals(request.Login);

        return null;
    }

    private static Expression<Func<Usuario, bool>>?
        SearchPredicateEmail
    (
        VerificarExistenciaUsuarioQuery request
    )
    {
        if (!string.IsNullOrEmpty(request.Email))
            return item => item
                .Email
                .Equals(request.Email);

        return null;
    }

    private static Expression<Func<Usuario, bool>> SearchPredicate
    (
        VerificarExistenciaUsuarioQuery request
    )
    {
        Expression<Func<Usuario, bool>> expression = item => false;

        var expressionId = SearchPredicateId(request);
        if (expressionId is not null)
            expression = expression.Or(expressionId);

        var expressionLogin = SearchPredicateLogin(request);
        if (expressionLogin is not null)
            expression = expression.Or(expressionLogin);

        var expressionEmail = SearchPredicateEmail(request);
        if (expressionEmail is not null)
            expression = expression.Or(expressionEmail);

        return expression;
    }
}