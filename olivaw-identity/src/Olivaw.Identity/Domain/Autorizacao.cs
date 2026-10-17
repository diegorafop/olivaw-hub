using System;
using Mps.OlivawCore.Domain;

namespace Olivaw.Identity.Domain;

public class Autorizacao : Entity
{
    public string Chave { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
