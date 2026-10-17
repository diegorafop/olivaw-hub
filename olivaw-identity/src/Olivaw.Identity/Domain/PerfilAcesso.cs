using System;
using Mps.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class PerfilAcesso : Entity
{
    public string Nome { get; set; } = string.Empty;
    public string NomeNormalizado { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
}
