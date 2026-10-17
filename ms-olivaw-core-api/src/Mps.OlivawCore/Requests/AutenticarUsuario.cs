using System;

namespace Mps.OlivawCore.Requests;

public class AutenticarUsuarioCommand
{
    public string Login { get; set; }
    public string Senha { get; set; }
}

public class AutenticarUsuarioCommandResponse
{
    public bool Ativo { get; set; }
    public Guid UsuarioId { get; set; }
    public string Token { get; set; }
    public string TokenRefresh { get; set; }
}