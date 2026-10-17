using Mps.Auth.Requests;
using Mps.Auth.Services;
using Mps.Auth.Settings;
using Mps.HolosCore.Api.Services;
using Mps.HolosCore.Api.Validation;
using Mps.HolosCore.Services;

namespace Mps.Auth.Api.Services;

public class ServiceNotificacao
(
    IHttpClientFactory factory,
    ISettingNotificacao setting,
    IProviderAuthentication service,
    ServiceResponseValidator validator
) : ServiceHttpClient
    (
        factory,
        setting,
        service,
        validator
    ),
    IServiceNotificacao
{
    public string DestinatarioRoute => _setting.Routes["destinatario"];
    public string UsuarioHubRoute => _setting.Routes["usuario-hub"];

    public string EmailRoute => _setting.Routes["email-envio"];

    public async Task<UpsertDestinatarioCommandResponse> UpsertDestinatarioAsync
    (
        UpsertDestinatarioCommand request,
        CancellationToken cancellationToken
    )
    {
        return await PostAsync<UpsertDestinatarioCommandResponse>
        (
            $"{DestinatarioRoute}/upsert",
            request,
            cancellationToken
        );
    }

    public async Task<EnfileirarUsuarioMessageCommandResponse>
        EnfileirarUsuarioMessageLoteAsync
    (
        IEnumerable<EnfileirarUsuarioMessageCommand> request,
        CancellationToken cancellationToken
    )
    {
        return await PostAsync<EnfileirarUsuarioMessageCommandResponse>
        (
            $"{UsuarioHubRoute}/lote",
            request,
            cancellationToken
        );
    }

    public async Task<EnfileirarUsuarioMessageCommandResponse>
        EnfileirarUsuarioMessageAsync
    (
        EnfileirarUsuarioMessageCommand request,
        CancellationToken cancellationToken
    )
    {
        return await PostAsync<EnfileirarUsuarioMessageCommandResponse>
        (
            $"{UsuarioHubRoute}",
            request,
            cancellationToken
        );
    }

    public async Task<EnfileirarEnviarEmailByEnderecoCommandResponse>
          EnfileirarEnviarEmailByEnderecoAsync
   (
        EnfileirarEnviarEmailByEnderecoCommand request,
        CancellationToken cancellationToken
   )
    {
        return await PostAsync<EnfileirarEnviarEmailByEnderecoCommandResponse>
        (
            $"{EmailRoute}",
            request,
            cancellationToken
        );
    }
}