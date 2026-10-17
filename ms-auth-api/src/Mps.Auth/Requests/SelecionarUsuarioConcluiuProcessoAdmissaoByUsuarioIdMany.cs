using System;
using System.Collections.Generic;

namespace Mps.Auth.Requests
{
    public class SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery
    {
        public IEnumerable<Guid> UsuarioIdMany { get; set; }
    }

    public class SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQueryResponse
    {
        public Guid UsuarioId { get; set; }
    }
}