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
            Id = entity.IdExterno,
            DataCadastro = entity.DataCadastro,
            PerfilAcessoId = entity.PerfilAcesso?.IdExterno ?? Guid.Empty,
            AutorizacaoId = entity.Autorizacao?.IdExterno ?? Guid.Empty
        };
    }
}
