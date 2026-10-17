using Mps.HolosCore.Domain;

namespace Mps.Auth.Domain;

public class UsuarioPerfilAcesso : Entity
{
    private Usuario _Usuario;
    private PerfilAcesso _PerfilAcesso;

    public int UsuarioId { get; set; }
    public virtual Usuario Usuario
    {
        get { return _Usuario; }
        set { _Usuario = value; SetUsuario(value); }
    }

    public int PerfilAcessoId { get; set; }
    public virtual PerfilAcesso PerfilAcesso
    {
        get { return _PerfilAcesso; }
        set { _PerfilAcesso = value; SetPerfilAcesso(value); }
    }

    private void SetUsuario
    (
        Usuario value
    )
    {
        _Usuario = value;
        UsuarioId = value == null ? 0 : value.Id;
    }

    private void SetPerfilAcesso
    (
        PerfilAcesso value
    )
    {
        _PerfilAcesso = value;
        PerfilAcessoId = value == null ? 0 : value.Id;
    }
}