using Mps.HolosCore.Contracts;
using System;

namespace Mps.Auth.Contracts;

public class EnfileirarUpsertDestinatarioContract : BaseContract
{
    public string Nome { get; set; }
    public string Endereco { get; set; }
    public Guid UsuarioLogadoId { get; set; }
}