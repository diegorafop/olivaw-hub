using System;
using Mps.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class PerfilAcessoAutorizacao : Entity
{
    public int PerfilAcessoId { get; set; }
    public int AutorizacaoId { get; set; }
    public PerfilAcesso? PerfilAcesso { get; set; }
    public Autorizacao? Autorizacao { get; set; }
}
