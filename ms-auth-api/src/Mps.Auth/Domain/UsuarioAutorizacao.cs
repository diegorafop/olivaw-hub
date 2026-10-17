using Mps.HolosCore.Domain;

namespace Mps.Auth.Domain;

public class UsuarioAutorizacao : Entity
{
    public int UsuarioId { get; set; }
    private Usuario _Usuario;
    public virtual Usuario Usuario { get { return _Usuario; } set { _Usuario = value; SetUsuario(value); } }

    public int AutorizacaoId { get; set; }
    private Autorizacao _Autorizacao;
    public virtual Autorizacao Autorizacao { get { return _Autorizacao; } set { SetAutorizacao(value); } }

    private void SetAutorizacao(Autorizacao value)
    {
        _Autorizacao = value;
        AutorizacaoId = value == null ? 0 : value.Id;
    }

    private void SetUsuario(Usuario value)
    {
        _Usuario = value;
        UsuarioId = value == null ? 0 : value.Id;
    }
}