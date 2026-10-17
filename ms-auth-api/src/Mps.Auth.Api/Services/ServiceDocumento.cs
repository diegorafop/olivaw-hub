using Mps.Auth.Api.Extensions.Https;
using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.Auth.Settings;
using Mps.HolosCore.Api.Services;
using Mps.HolosCore.Api.Validation;
using Mps.HolosCore.Services;

namespace Mps.Auth.Api.Services;

public class ServiceDocumento
(
    IHttpClientFactory factory,
    ISettingDocumento setting,
    IProviderAuthentication service,
    ServiceResponseValidator validator
) : ServiceHttpClient
    (
        factory,
        setting,
        service,
        validator
    ),
    IServiceDocumento
{
    public string AssuntoProcessoAdmissaoRoute => _setting.Routes["assunto-processo-admissao"];


    public async Task<IEnumerable<SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQueryResponse>>
        SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyAsync
    (
        SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQuery request,
        CancellationToken cancellationToken
    )
    {
        var route = $"{AssuntoProcessoAdmissaoRoute}/concluido";

        return await GetAsync<IEnumerable<SelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyQueryResponse>>
        (
            request.ToSelecionarUsuarioConcluiuProcessoAdmissaoByUsuarioIdManyRoute(route),
            cancellationToken
        );
    }
}