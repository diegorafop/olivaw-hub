using Mps.HolosCore.Domain;
using System.Collections.Generic;

namespace Mps.Auth.Domain;

public class PerfilAcesso : Entity
{
    public string Nome { get; set; }
    public string? Descricao { get; set; }

    public IEnumerable<PerfilAcessoAutorizacao> Autorizacaos { get; set; }
}