using Mps.HolosCore.Domain;

namespace Mps.Auth.Domain;

public class Autorizacao : Entity
{
    public string Tipo { get; set; }
    public string Valor { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public bool Perfil { get; set; }
}