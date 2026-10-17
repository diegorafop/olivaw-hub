using Mps.Auth.Requests;
using Mps.HolosCore.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mps.Auth.Services;

public interface IServiceUsuario : IServiceHttpClient
{
    public string UsuarioRoute { get; }
    public string UsuarioCargoRoute { get; }


    public Task<SelecionarUsuarioCargoByUsuarioIdQueryResponse>
        SelecionarUsuarioCargoByUsuarioIdAsync
    (
        Guid id,
        CancellationToken cancellationToken
    );

    public Task<IEnumerable<SelecionarUsuarioCargoByFilterQueryResponse>>
        SelecionarUsuarioCargoByFilterAsync
    (
        SelecionarUsuarioCargoByFilterQuery request,
        CancellationToken cancellationToken
    );

    public Task<UpsertDocumentoUsuarioPreferenciaCommandResponse>
        UpsertDocumentoUsuarioPreferenciaAsync
    (
        UpsertDocumentoUsuarioPreferenciaCommand request,
        CancellationToken cancellationToken
    );
}