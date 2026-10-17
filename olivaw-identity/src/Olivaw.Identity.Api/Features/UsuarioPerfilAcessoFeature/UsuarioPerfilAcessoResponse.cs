using Olivaw.Identity.Domain;

namespace Olivaw.Identity.Api.Features.UsuarioPerfilAcessoFeature;

public sealed class UsuarioPerfilAcessoResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid PerfilAcessoId { get; set; }

    internal static UsuarioPerfilAcessoResponse From(UsuarioPerfilAcesso entity)
    {
        return new UsuarioPerfilAcessoResponse
        {
            Id = entity.IdExterno,
            DataCadastro = entity.DataCadastro,
            UsuarioId = entity.Usuario?.IdExterno ?? Guid.Empty,
            PerfilAcessoId = entity.PerfilAcesso?.IdExterno ?? Guid.Empty
        };
    }
}
