using System;
using Mps.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class Usuario : Entity
{
    public string Login { get; set; } = string.Empty;
    public string LoginNormalizado { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public int FalhasLogin { get; set; }
    public DateTime? BloqueadoAte { get; set; }
    public bool Ativo { get; set; } = true;
    public bool Root { get; set; }
    public Guid VersaoSeguranca { get; set; } = Guid.NewGuid();
}
