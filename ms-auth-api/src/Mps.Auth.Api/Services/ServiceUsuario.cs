using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.Auth.Settings;
using Mps.HolosCore.Api.Services;
using Mps.HolosCore.Api.Validation;
using Mps.HolosCore.Services;

namespace Mps.Auth.Api.Services;

public class ServiceUsuario
(
    IHttpClientFactory factory,
    ISettingUsuario setting,
    IProviderAuthentication service,
    ServiceResponseValidator validator
) : ServiceHttpClient
    (
        factory,
        setting,
        service,
        validator
    ),
    IServiceUsuario
{
    public string UsuarioRoute => _setting.Routes["usuario"];
    public string UsuarioCargoRoute => _setting.Routes["usuario-cargo"];
    public string DocumentoUsuarioPreferenciaRoute => _setting.Routes["documento-usuario-preferencia"];


    public async Task<SelecionarUsuarioCargoByUsuarioIdQueryResponse>
        SelecionarUsuarioCargoByUsuarioIdAsync
    (
        Guid id,
        CancellationToken cancellationToken
    )
    {
        return await GetAsync<SelecionarUsuarioCargoByUsuarioIdQueryResponse>
        (
            $"{UsuarioCargoRoute}/usuario/{id}",
            cancellationToken
        );
    }

    public async Task<UpsertDocumentoUsuarioPreferenciaCommandResponse>
        UpsertDocumentoUsuarioPreferenciaAsync
    (
        UpsertDocumentoUsuarioPreferenciaCommand request,
        CancellationToken cancellationToken
    )
    {
        return await PutAsync<UpsertDocumentoUsuarioPreferenciaCommandResponse>
        (
            $"{DocumentoUsuarioPreferenciaRoute}",
            request,
            cancellationToken
        );
    }

    public async Task<IEnumerable<SelecionarUsuarioCargoByFilterQueryResponse>>
        SelecionarUsuarioCargoByFilterAsync
    (
        SelecionarUsuarioCargoByFilterQuery request,
        CancellationToken cancellationToken
    )
    {
        return await GetAsync<IEnumerable<SelecionarUsuarioCargoByFilterQueryResponse>>
        (
            request.GetRoute(UsuarioCargoRoute),
            cancellationToken
        );
    }
}