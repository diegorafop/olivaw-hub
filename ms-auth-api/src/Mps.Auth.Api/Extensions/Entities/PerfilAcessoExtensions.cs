using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;
using Mps.Auth.Api.Features.PerfilAcessoFeature.Commands;
using Mps.Auth.Api.Features.PerfilAcessoFeature.Queries;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Collections;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Extensions.Entities;

public static class PerfilAcessoExtensions
{
    public static Expression<Func<PerfilAcesso, bool>> SearchPredicate
    (
        this SelecionarPerfilAcessoByFilterQuery request
    )
    {
        Expression<Func<PerfilAcesso, bool>> predicate = item => true;

        if (!string.IsNullOrEmpty(request.Nome))
            predicate = predicate.And(item => item.Nome.Contains(request.Nome));

        return predicate;
    }

    public static Expression<Func<PerfilAcessoAutorizacao, bool>> SearchPredicate
    (
        this SelecionarPerfilAcessoAutorizacaoByFilterQuery request
    )
    {
        Expression<Func<PerfilAcessoAutorizacao, bool>> predicate = item => true;

        if (request.AutorizacaoId.HasValue)
            predicate = predicate.And(item => item
                .Autorizacao
                .IdExterno
                .Equals(request.AutorizacaoId.Value));

        if (request.PerfilAcessoId.HasValue)
            predicate = predicate.And(item => item
                .PerfilAcesso
                .IdExterno
                .Equals(request.PerfilAcessoId.Value));

        return predicate;
    }

    public static Expression<Func<PerfilAcesso, bool>>
        SearchPredicateByPerfilBasico
    (
        this AutenticarImpersonateCommand request
    )
    {
        return SearchPredicateByNome("Perfil básico");
    }

    private static Expression<Func<PerfilAcesso, bool>>
        SearchPredicateByNome
    (
        string perfil
    )
    {
        return item => item
            .Nome
            .Equals(perfil);
    }

    public static void Update
    (
        this AtualizarPerfilAcessoCommand request,
        PerfilAcesso entity
    )
    {
        entity.Nome = request.Nome;
        entity.Descricao = request.Descricao;
    }
}