using System;
using Mps.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class UsuarioPerfilAcesso : Entity
{
    public int UsuarioId { get; set; }
    public int PerfilAcessoId { get; set; }
    public Usuario? Usuario { get; set; }
    public PerfilAcesso? PerfilAcesso { get; set; }
}
