using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.PerfilAcessoAutorizacaoFeature;

public sealed class PerfilAcessoAutorizacaoResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public Guid PerfilAcessoId { get; set; }
    public Guid AutorizacaoId { get; set; }

    internal static PerfilAcessoAutorizacaoResponse From(PerfilAcessoAutorizacao entity)
    {
        return new PerfilAcessoAutorizacaoResponse
        {
            Id = entity.ExternalId,
            DataCadastro = entity.CreatedAt,
            PerfilAcessoId = entity.PerfilAcesso?.ExternalId ?? Guid.Empty,
            AutorizacaoId = entity.Autorizacao?.ExternalId ?? Guid.Empty
        };
    }
}
