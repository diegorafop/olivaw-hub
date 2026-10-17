using Mps.Auth.Api.Features.AutenticacaoFeature.Commands;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Commands;
using Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Queries;
using Mps.Auth.Api.Features.UsuarioFeature.Commands;
using Mps.Auth.Api.Features.UsuarioFeature.Internals;
using Mps.Auth.Api.Features.UsuarioFeature.Queries;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;

namespace Mps.Auth.Api.Extensions.Entities;

public static class UsuarioAutorizacaoExtensions
{
    public static IList<UsuarioAutorizacao> ToDomain
    (
        this AutenticarUsuarioComGovCommand request,
        InserirUsuarioInternalCommandResponse response,
        Autorizacao autorizacao
    )
    {
        UsuarioAutorizacao entity = EntityExtensions
            .ToDomain<AutenticarUsuarioComGovCommand,
                UsuarioAutorizacao>(request);

        entity.Usuario = response.Usuario;
        entity.Autorizacao = autorizacao;

        return [entity];
    }

    public static UsuarioAutorizacao ToDomain
    (
        this AutenticarImpersonateCommand request,
        Usuario usuario,
        Autorizacao autorizacao
    )
    {
        UsuarioAutorizacao entity = EntityExtensions
            .ToDomain<AutenticarImpersonateCommand,
                UsuarioAutorizacao>(request);

        entity.Usuario = usuario;
        entity.Autorizacao = autorizacao;

        return entity;
    }

    public static IEnumerable<UsuarioAutorizacao> ToDomain
    (
        this AutenticarUsuarioComGovAutorizacaoManyCommand request,
        Usuario usuario,
        IEnumerable<Autorizacao> autorizacaoMany
    )
    {
        return autorizacaoMany.Select(autorizacao =>
        {
            UsuarioAutorizacao entity = EntityExtensions
                .ToDomain<AutenticarUsuarioComGovAutorizacaoManyCommand,
                    UsuarioAutorizacao>(request);

            entity.Usuario = usuario;
            entity.Autorizacao = autorizacao;

            return entity;
        });
    }

    public static SelecionarUsuarioAutorizacaoByFilterQueryResponse
        ToSelecionarUsuarioAutorizacaoByFilterQueryResponse
    (
        this UsuarioAutorizacao entity
    )
    {
        var response = entity.Map<SelecionarUsuarioAutorizacaoByFilterQueryResponse>();

        response.AutorizacaoId = entity.Autorizacao.IdExterno;
        response.UsuarioId = entity.Usuario.IdExterno;

        return response;
    }

    public static SelecionarUsuarioAutorizacaoByIdQueryResponse
        ToSelecionarUsuarioAutorizacaoByIdQueryResponse
    (
        this UsuarioAutorizacao entity
    )
    {
        var response = entity.Map<SelecionarUsuarioAutorizacaoByIdQueryResponse>();

        response.AutorizacaoId = entity.Autorizacao.IdExterno;
        response.UsuarioId = entity.Usuario.IdExterno;

        return response;
    }

    public static UsuarioAutorizacao ToDomain
    (
        this InserirUsuarioAutorizacaoCommand request,
        Usuario usuario,
        Autorizacao autorizacao
    )
    {
        UsuarioAutorizacao entity = EntityExtensions
            .ToDomain<InserirUsuarioAutorizacaoCommand,
                UsuarioAutorizacao>(request);

        entity.UsuarioId = usuario.Id;
        entity.AutorizacaoId = autorizacao.Id;

        return entity;
    }

    public static UsuarioAutorizacao ToDomain
    (
        this UpsertUsuarioHolosCommand request,
        Usuario usuario,
        Autorizacao autorizacao
    )
    {
        UsuarioAutorizacao entity = EntityExtensions
            .ToDomain<UpsertUsuarioHolosCommand,
                UsuarioAutorizacao>(request);

        entity.UsuarioId = usuario.Id;
        entity.AutorizacaoId = autorizacao.Id;

        return entity;
    }

    public static void Update
    (
        this UsuarioAutorizacao entity,
        Autorizacao autorizacao,
        Guid usuarioLogadoId
    )
    {
        entity.AutorizacaoId = autorizacao.Id;
        entity.UsuarioLogadoId = usuarioLogadoId;
    }


    public static IEnumerable<SelecionarUsuarioByIdManyQueryResponse>
        ToManySelecionarManyUsuarioByIdManyQueryItemResponse
    (
        this IEnumerable<UsuarioAutorizacao> usuarioAutorizacaoMany
    )
    {
        return [.. usuarioAutorizacaoMany
            .Where(item => item
                .Autorizacao
                .Tipo
                .Equals("matricula"))
            .Select(item =>
            {
                var response = item
                    .Usuario
                    .Map<SelecionarUsuarioByIdManyQueryResponse>();

                response.Matricula = int.Parse(item.Autorizacao.Valor);

                return response;
            })];
    }

    public static bool CanBeRemoved
    (
        this UsuarioAutorizacao entity,
        IConfiguration configuration
    )
    {
        var cantBeRemoved = configuration
            .GetSection("constants:roles-imutaveis")
            .Get<IEnumerable<string>>();

        if (cantBeRemoved is null)
            return false;

        return !cantBeRemoved.Contains(entity.Autorizacao.Valor);
    }
}