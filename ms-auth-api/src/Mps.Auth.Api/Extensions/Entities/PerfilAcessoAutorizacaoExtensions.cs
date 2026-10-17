using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Commands;
using Mps.Auth.Api.Features.PerfilAcessoAutorizacaoFeature.Queries;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;

namespace Mps.Auth.Api.Extensions.Entities;

public static class PerfilAcessoAutorizacaoExtensions
{
    public static PerfilAcessoAutorizacao ToDomain
    (
        this InserirPerfilAcessoAutorizacaoCommand request,
        PerfilAcesso perfilAcesso,
        Autorizacao autorizacao
    )
    {
        PerfilAcessoAutorizacao entity = EntityExtensions
            .ToDomain<InserirPerfilAcessoAutorizacaoCommand,
                PerfilAcessoAutorizacao>(request);

        entity.PerfilAcessoId = perfilAcesso.Id;
        entity.AutorizacaoId = autorizacao.Id;

        return entity;
    }

    public static void Update
    (
        this AtualizarPerfilAcessoAutorizacaoCommand request,
        PerfilAcessoAutorizacao entity,
        PerfilAcesso perfilAcesso,
        Autorizacao autorizacao
    )
    {
        entity.PerfilAcessoId = perfilAcesso.Id;
        entity.AutorizacaoId = autorizacao.Id;
        entity.UsuarioLogadoId = request.UsuarioLogadoId;
    }

    public static SelecionarPerfilAcessoAutorizacaoByIdQueryResponse
        ToSelecionarPerfilAcessoAutorizacaoByIdQueryResponse
    (
       this PerfilAcessoAutorizacao entity
    )
    {
        var response = entity.Map<SelecionarPerfilAcessoAutorizacaoByIdQueryResponse>();

        response.PerfilAcessoId = entity
            .PerfilAcesso
            .IdExterno;

        response.AutorizacaoId = entity
            .Autorizacao
            .IdExterno;

        return response;
    }

    public static IEnumerable<SelecionarPerfilAcessoAutorizacaoByFilterQueryResponse>
        ToSelecionarPerfilAcessoAutorizacaoByFilterQueryResponse
    (
        this IEnumerable<PerfilAcessoAutorizacao> entities
    )
    {
        return entities.Select(item =>
        {
            return item.ToSelecionarPerfilAcessoAutorizacaoByFilterQueryResponse();
        });
    }

    private static SelecionarPerfilAcessoAutorizacaoByFilterQueryResponse
        ToSelecionarPerfilAcessoAutorizacaoByFilterQueryResponse
    (
        this PerfilAcessoAutorizacao item
    )
    {
        var response = item.Map<SelecionarPerfilAcessoAutorizacaoByFilterQueryResponse>();

        response.PerfilAcessoId = item
            .PerfilAcesso
            .IdExterno;

        response.AutorizacaoId = item
            .Autorizacao
            .IdExterno;

        return response;
    }
}