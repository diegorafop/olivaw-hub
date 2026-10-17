using Microsoft.Net.Http.Headers;
using Mps.OlivawCore.Settings;

namespace Mps.OlivawCore.Api.Extensions.Https;

public static class HttpClientExtensions
{
    public static string GetRoute
    (
        this HttpClient httpClient,
        string route
    )
    {
        if (httpClient is null)
            return route;

        var baseAddress = httpClient.BaseAddress;

        if (baseAddress.HasAbsolutePath())
            return route.TrimStart('/');

        return route;
    }

    private static bool HasAbsolutePath
    (
        this Uri? uri
    )
    {
        if (uri is null)
            return false;

        var value = uri.AbsolutePath;

        if (string.IsNullOrEmpty(value))
            return false;

        value = value
            .Replace("/", string.Empty)
            .Trim();

        if (string.IsNullOrEmpty(value))
            return false;

        return true;
    }

    public static Action<HttpClient> GetHttpClientFactory
    (
        this ISetting setting
    )
    {
        return httpClient =>
        {
            httpClient.Timeout = TimeSpan.FromSeconds(setting.HttpClientTimeout);
            httpClient.BaseAddress = new Uri(setting.BaseAddress);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        };
    }
}