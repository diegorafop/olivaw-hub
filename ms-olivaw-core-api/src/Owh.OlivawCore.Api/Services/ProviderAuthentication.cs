using Microsoft.Extensions.Caching.Memory;
using Owh.OlivawCore.Api.Extensions.Https;
using Owh.OlivawCore.Requests;
using Owh.OlivawCore.Services;
using Owh.OlivawCore.Settings;
using System.Net.Http.Headers;

namespace Owh.OlivawCore.Api.Services;

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
        var response = await GetAuthenticateUserCommandResponseAsync
        (
            cancellationToken
        );

        response = await ValidateAuthenticateUserCommandResponseAsync
        (
            response,
            cancellationToken
        );

        return GetAuthenticationHeaderValue(response);
    }

    private async Task<AuthenticateUserCommandResponse>
        GetAuthenticateUserCommandResponseAsync
    (
        CancellationToken cancellationToken
    )
    {
        return await _memoryCache.GetOrCreateAsync
        (
            GetAuthenticationKey(),
            async item =>
            {
                item.SlidingExpiration = TimeSpan.FromMinutes
                (
                    _settingProviderAuthentication.CacheSlidingExpiration
                );

                return await PostAsync<AuthenticateUserCommandResponse>
                (
                    AutenticarRoute,
                    GetAuthenticateUserCommand(),
                    cancellationToken
                );
            }
        );
    }

    private async Task<AuthenticateUserCommandResponse> ValidateAuthenticateUserCommandResponseAsync
    (
        AuthenticateUserCommandResponse? response,
        CancellationToken cancellationToken
    )
    {
        if (!response.JwtTokenIsValid())
        {
            _memoryCache.Remove(GetAuthenticationKey());
            response = await GetAuthenticateUserCommandResponseAsync(cancellationToken);
        }

        return response;
    }

    private string GetAuthenticationKey()
    {
        return $"{_settingProviderAuthentication.ServiceName}-{_settingProviderAuthentication.CacheKey}";
    }

    private AuthenticationHeaderValue GetAuthenticationHeaderValue
    (
        AuthenticateUserCommandResponse response
    )
    {
        return new AuthenticationHeaderValue
        (
            _setting.AuthenticationType,
            response.Token
        );
    }

    private AuthenticateUserCommand GetAuthenticateUserCommand()
    {
        return new AuthenticateUserCommand
        {
            Login = _settingProviderAuthentication.Login,
            Password = _settingProviderAuthentication.Password
        };
    }
}