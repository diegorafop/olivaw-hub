namespace Mps.Auth.Api.Features.EntityFeature.Commands;

public abstract class EntityCommand
{
    internal Guid UsuarioLogadoId { get; set; }
    public EntityCommand() { }
    public EntityCommand
    (
        Guid usuarioLogadoId
    )
    {
        UsuarioLogadoId = usuarioLogadoId;
    }
}