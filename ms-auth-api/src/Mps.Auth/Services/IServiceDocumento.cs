using Mps.Auth.Requests;
using Mps.HolosCore.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mps.Auth.Services;

public interface IServiceDocumento : IServiceHttpClient
{
    public string AssuntoProcessoAdmissaoRoute { get; }


    public Task<IEnumerable<SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQueryResponse>>
        SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyAsync
    (
        SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery request,
        CancellationToken cancellationToken
    );
}