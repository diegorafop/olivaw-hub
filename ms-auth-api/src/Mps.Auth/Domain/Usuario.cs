using Mps.Auth.Domain.Enum;
using Mps.HolosCore.Domain;
using System.Collections.Generic;

namespace Mps.Auth.Domain;

public class Usuario : Entity
{
    public string Login { get; set; }
    public string? Senha { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string? IntegracaoId { get; set; }
    public StatusIntegracaoEnum StatusIntegracaoEnum { get; set; }

    public IEnumerable<UsuarioAutorizacao> Autorizacoes { get; set; }
    public IEnumerable<UsuarioPerfilAcesso> Perfils { get; set; }
}