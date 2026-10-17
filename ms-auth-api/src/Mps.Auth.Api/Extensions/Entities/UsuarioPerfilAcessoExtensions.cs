
using Mps.Auth.Api.Features.UsuarioFeature.Internals;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Commands;
using Mps.Auth.Api.Features.UsuarioPerfilAcessoFeature.Queries;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Extensions.Entities;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Extensions.Entities;

public static class UsuarioPerfilAcessoExtensions
{
    public static Expression<Func<UsuarioPerfilAcesso, bool>> SearchPredicate
    (
        this SelecionarUsuarioPerfilAcessoByFilterQuery request
    )
    {
        Expression<Func<Domain.UsuarioPerfilAcesso, bool>> predicate = item => true;

        if (request.UsuarioId.HasValue)
            predicate = predicate.And(item => item
                .Usuario
                .IdExterno
                .Equals(request.UsuarioId.Value));

        if (request.PerfilAcessoId.HasValue)
            predicate = predicate.And(item => item
                .PerfilAcesso
                .IdExterno
                .Equals(request.PerfilAcessoId.Value));

        return predicate;
    }

    public static UsuarioPerfilAcesso ToDomain
    (
        this InserirUsuarioPerfilAcessoCommand request,
        PerfilAcesso perfilAcesso,
        Usuario usuario
    )
    {
        UsuarioPerfilAcesso entity = EntityExtensions
            .ToDomain<InserirUsuarioPerfilAcessoCommand,
                UsuarioPerfilAcesso>(request);

        entity.PerfilAcessoId = perfilAcesso.Id;
        entity.UsuarioId = usuario.Id;

        return entity;
    }

    public static void Update
    (
        this AtualizarUsuarioPerfilAcessoCommand request,
        UsuarioPerfilAcesso entity,
        PerfilAcesso perfilAcesso,
        Usuario usuario
    )
    {
        entity.PerfilAcessoId = perfilAcesso.Id;
        entity.UsuarioId = usuario.Id;
        entity.UsuarioLogadoId = request.UsuarioLogadoId;
    }

    public static SelecionarUsuarioPerfilAcessoByIdQueryResponse
        ToSelecionarUsuarioPerfilAcessoByIdQueryResponse
    (
       this UsuarioPerfilAcesso entity
    )
    {
        var response = entity.Map<SelecionarUsuarioPerfilAcessoByIdQueryResponse>();

        response.PerfilAcessoId = entity.GetPerfilAcessoId();
        response.UsuarioId = entity.GetUsuarioId();

        return response;
    }

    public static IEnumerable<SelecionarUsuarioPerfilAcessoByFilterQueryResponse>
        ToSelecionarUsuarioPerfilAcessoByFilterQueryResponse
    (
        this IEnumerable<UsuarioPerfilAcesso> entities
    )
    {
        return entities.Select(item =>
        {
            var response = item.Map<SelecionarUsuarioPerfilAcessoByFilterQueryResponse>();

            //TODO - necessário pois o retorno é "Guid?"
            if (item.PerfilAcesso is not null)
                response.PerfilAcessoId = item.GetPerfilAcessoId();

            response.UsuarioId = item.GetUsuarioId();

            return response;
        });
    }

    public static UsuarioPerfilAcesso ToDomain
    (
        this InserirUsuarioInternalCommand request,
        Usuario usuario,
        PerfilAcesso perfilAcesso
    )
    {
        var entity = EntityExtensions
            .ToDomain<InserirUsuarioInternalCommand,
                UsuarioPerfilAcesso>(request);

        entity.PerfilAcesso = perfilAcesso;
        entity.Usuario = usuario;

        return entity;
    }

    public static Guid GetPerfilAcessoId
    (
        this UsuarioPerfilAcesso entity
    )
    {
        if (entity.PerfilAcesso is null)
            return Guid.Empty;

        return entity
            .PerfilAcesso
            .IdExterno;
    }

    public static Guid GetUsuarioId
    (
        this UsuarioPerfilAcesso entity
    )
    {
        if (entity.Usuario is null)
            return Guid.Empty;

        return entity
            .Usuario
            .IdExterno;
    }
}