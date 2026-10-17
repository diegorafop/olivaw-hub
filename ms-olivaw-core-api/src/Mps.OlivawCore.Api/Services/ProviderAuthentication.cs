using Microsoft.Extensions.Caching.Memory;
using Mps.OlivawCore.Api.Extensions.Https;
using Mps.OlivawCore.Requests;
using Mps.OlivawCore.Services;
using Mps.OlivawCore.Settings;
using System.Net.Http.Headers;

namespace Mps.OlivawCore.Api.Services;

public class ProviderAuthentication
(
    ISettingProviderAuthentication setting,
    IHttpClientFactory factory,
    IMemoryCache cache
) : ServiceHttpClient
(
    factory,
    setting
), IProviderAuthentication
{
    private readonly ISettingProviderAuthentication _settingProviderAuthentication = setting;

    private readonly IMemoryCache _memoryCache = cache
        ?? throw new ArgumentNullException(nameof(cache));

    public string AutenticarRoute => _setting.Routes["autenticar-usuario"];

    public async Task<AuthenticationHeaderValue> AutenticarAsync
    (
        CancellationToken cancellationToken
    )
    {
        var response = await GetAutenticarUsuarioCommandResponseAsync
        (
            cancellationToken
        );

        response = await ValidateAutenticarUsuarioCommandResponseAsync
        (
            response,
            cancellationToken
        );

        return GetAuthenticationHeaderValue(response);
    }

    private async Task<AutenticarUsuarioCommandResponse>
        GetAutenticarUsuarioCommandResponseAsync
    (
        CancellationToken cancellationToken
    )
    {
        return await _memoryCache.GetOrCreateAsync
        (
            GetAutenticacaoKey(),
            async item =>
            {
                item.SlidingExpiration = TimeSpan.FromMinutes
                (
                    _settingProviderAuthentication.CacheSlidingExpiration
                );

                return await PostAsync<AutenticarUsuarioCommandResponse>
                (
                    AutenticarRoute,
                    GetAutenticarUsuarioCommand(),
                    cancellationToken
                );
            }
        );
    }

    private async Task<AutenticarUsuarioCommandResponse> ValidateAutenticarUsuarioCommandResponseAsync
    (
        AutenticarUsuarioCommandResponse? response,
        CancellationToken cancellationToken
    )
    {
        if (!response.JwtTokenIsValid())
        {
            _memoryCache.Remove(GetAutenticacaoKey());
            response = await GetAutenticarUsuarioCommandResponseAsync(cancellationToken);
        }

        return response;
    }

    private string GetAutenticacaoKey()
    {
        return $"{_settingProviderAuthentication.ServiceName}-{_settingProviderAuthentication.CacheKey}";
    }

    private AuthenticationHeaderValue GetAuthenticationHeaderValue
    (
        AutenticarUsuarioCommandResponse response
    )
    {
        return new AuthenticationHeaderValue
        (
            _setting.AuthenticationType,
            response.Token
        );
    }

    private AutenticarUsuarioCommand GetAutenticarUsuarioCommand()
    {
        return new AutenticarUsuarioCommand
        {
            Login = _settingProviderAuthentication.Login,
            Senha = _settingProviderAuthentication.Senha
        };
    }
}