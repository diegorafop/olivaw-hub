using Mps.HolosCore.Domain;

namespace Mps.Auth.Domain;

public class PerfilAcessoAutorizacao : Entity
{
    private Autorizacao _Autorizacao { get; set; }
    private PerfilAcesso _PerfilAcesso { get; set; }

    public int AutorizacaoId { get; set; }
    public virtual Autorizacao Autorizacao
    {
        get { return _Autorizacao; }
        set { _Autorizacao = value; SetAutorizacao(value); }
    }

    public int PerfilAcessoId { get; set; }
    public virtual PerfilAcesso PerfilAcesso
    {
        get { return _PerfilAcesso; }
        set { _PerfilAcesso = value; SetPerfilAcesso(value); }
    }

    private void SetPerfilAcesso
    (
        PerfilAcesso entity
    )
    {
        _PerfilAcesso = entity;
        PerfilAcessoId = entity == null ? 0 : entity.Id;
    }

    private void SetAutorizacao
    (
        Autorizacao entity
    )
    {
        _Autorizacao = entity;
        AutorizacaoId = entity == null ? 0 : entity.Id;
    }
}