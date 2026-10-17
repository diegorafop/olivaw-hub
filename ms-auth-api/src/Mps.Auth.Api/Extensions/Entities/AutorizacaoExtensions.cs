using Mps.Auth.Api.Extensions.Hostings;
using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.Auth.Api.Features.AutorizacaoFeature.Commands;
using Mps.Auth.Api.Features.AutorizacaoFeature.Queries;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Extensions.Https;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Mps.Auth.Api.Extensions.Entities;

public static class AutorizacaoExtensions
{
    public static Expression<Func<Autorizacao, bool>> SearchPredicate
    (
        this SelecionarAutorizacaoByFilterQuery request
    )
    {
        Expression<Func<Autorizacao, bool>> predicate = item => true;

        if (!string.IsNullOrEmpty(request.Tipo))
            predicate = predicate.And(item => item
                .Tipo
                .Contains(request.Tipo));

        if (!string.IsNullOrEmpty(request.Valor))
            predicate = predicate.And(item => item
                .Valor
                .Contains(request.Valor));

        return predicate;
    }

    public static Expression<Func<Autorizacao, bool>> SearchPredicateByRoleNivelConta
    (
        this AutenticarUsuarioComGovCommand request,
        string nivelConta
    )
    {
        if (request is null)
            return item => true;

        return SearchPredicateByRole(nivelConta);
    }

    public static Expression<Func<Autorizacao, bool>> SearchPredicateByRoleOverride
    (
        this AutenticarImpersonateCommand request
    )
    {
        if (request is null)
            return item => true;

        return SearchPredicateByRole("AuthorityOverride");
    }

    public static Expression<Func<Autorizacao, bool>> SearchPredicateByList
    (
        this UpsertUsuarioHolosCommand request,
        List<string> itens
    )
    {
        if (request is null)
            return item => true;

        return item => itens.Contains(item.Valor);
    }

    public static Expression<Func<Autorizacao, bool>> SearchPredicateByList
    (
        this AutenticarImpersonateCommand request,
        List<string> itens
    )
    {
        if (request is null)
            return item => true;

        return item => itens.Contains(item.Valor);
    }

    public static Expression<Func<Autorizacao, bool>> SearchPredicateByRoleNivelConta
    (
        this AutenticarUsuarioComGovAutorizacaoManyCommand request,
        string autorizacao
    )
    {
        if (request is null)
            return item => true;

        return SearchPredicateByRole(autorizacao);
    }

    private static Expression<Func<Autorizacao, bool>> SearchPredicateByRole
    (
        string role
    )
    {
        return item => item
            .Valor
            .Equals(role);
    }

    public static Expression<Func<Autorizacao, bool>> SearchPredicate
    (
        this InserirAutorizacaoCommand request
    )
    {
        return item => item
            .Tipo
            .Equals(request.Tipo)
            && item
                .Valor
                .Equals(request.Valor);
    }

    public static void Update
    (
        this AtualizarAutorizacaoCommand request,
        Autorizacao entity
    )
    {
        entity.Nome = request.Nome;
        entity.Tipo = request.Tipo;
        entity.Valor = request.Valor;
        entity.Perfil = request.Perfil;
        entity.Descricao = request.Descricao;

        entity.Update(request.UsuarioLogadoId);
    }

    public static IEnumerable<Autorizacao> ToAutorizacaos
    (
        this IEnumerable<Claim> claims,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        return
        [
            GetAutorizacaoMatricula(claims, request, configuration),
            GetAutorizacaoEmissor(claims, request, configuration)
        ];
    }

    public static IEnumerable<Autorizacao> ToAutorizacaos
    (
        this IEnumerable<Claim> claims,
        AutenticarUsuarioComGovCommand request,
        IConfiguration configuration
    )
    {
        return
        [
            GetAutorizacaoEmissor
            (
                claims,
                request,
                configuration
            )
        ];
    }

    private static Autorizacao GetAutorizacaoMatricula
    (
        IEnumerable<Claim> claims,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        Autorizacao autorizacao = EntityExtensions
            .ToDomain<AutenticarUsuarioComAdCommand,
                Autorizacao>(request);

        var tipo = configuration.GetValue<string>
        (
            "authentication:oidc:custom-claims:matricula"
        );

        autorizacao.Tipo = string.IsNullOrEmpty(tipo) ? string.Empty : tipo;

        autorizacao.Valor = TryGetMatricula
        (
            claims,
            request,
            configuration
        );

        return autorizacao;
    }

    private static string GetMatricula
    (
        AutenticarUsuarioComAdCommand request
    )
    {
        if (string.IsNullOrEmpty(request.Matricula))
            return string.Empty;

        return request
            .Matricula
            .Trim();
    }

    private static string TryGetMatricula
    (
        IEnumerable<Claim> claims,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        string matricula = GetValue
        (
            claims,
            configuration,
            "authentication:oidc:custom-claims:matricula"
        );

        if (string.IsNullOrEmpty(matricula))
            return GetMatricula(request);

        return matricula;
    }

    private static string GetValue
    (
        IEnumerable<Claim> claims,
        IConfiguration configuration,
        string claimName
    )
    {
        var value = claims.GetValue(configuration.GetValue<string>(claimName));

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Trim();
    }

    private static Autorizacao GetAutorizacaoEmissor
    (
        IEnumerable<Claim> claims,
        AutenticarUsuarioComAdCommand request,
        IConfiguration configuration
    )
    {
        Autorizacao autorizacao = EntityExtensions
            .ToDomain<AutenticarUsuarioComAdCommand,
                Autorizacao>(request);

        var tipo = configuration.GetValue<string>
        (
            "authentication:oidc:custom-claims:emissor"
        );

        autorizacao.Tipo = string.IsNullOrEmpty(tipo) ? string.Empty : tipo;

        autorizacao.Valor = claims.GetValue
        (
            configuration.GetValue<string>("authentication:oidc:issuer")
        );

        return autorizacao;
    }

    private static Autorizacao GetAutorizacaoEmissor
    (
        IEnumerable<Claim> claims,
        AutenticarUsuarioComGovCommand request,
        IConfiguration configuration
    )
    {
        Autorizacao autorizacao = EntityExtensions
            .ToDomain<AutenticarUsuarioComGovCommand,
                Autorizacao>(request);

        autorizacao.Tipo = configuration.GetTipoByIssuer();

        autorizacao.Valor = claims.GetValue
        (
            configuration.GetValue<string>("authentication:oidc:issuer")
        );

        return autorizacao;
    }
}