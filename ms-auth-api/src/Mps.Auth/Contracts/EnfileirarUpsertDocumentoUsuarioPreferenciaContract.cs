using Mps.HolosCore.Contracts;
using System;

namespace Mps.Auth.Contracts;

public class EnfileirarUpsertDocumentoUsuarioPreferenciaContract : BaseContract
{
    public bool Interessado { get; set; }
    public Guid UsuarioId { get; set; }
}