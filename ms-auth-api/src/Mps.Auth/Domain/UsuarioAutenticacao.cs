using Mps.HolosCore.Domain;

namespace Mps.Auth.Domain;

public class UsuarioAutenticacao : Entity
{
    public string Token { get; set; }

    private Usuario _Usuario;

    public int UsuarioId { get; set; }
    public virtual Usuario Usuario
    {
        get { return _Usuario; }
        set { _Usuario = value; SetUsuario(value); }
    }

    private void SetUsuario
    (
        Usuario value
    )
    {
        UsuarioId = value == null ? 0 : value.Id;
    }
}