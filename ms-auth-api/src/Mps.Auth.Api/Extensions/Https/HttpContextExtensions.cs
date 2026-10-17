namespace Mps.Auth.Api.Extensions.Https;

public static class HttpContextExtensions
{
    internal static string GetToken
    (
        this HttpContext httpContext
    )
    {
        if (httpContext is null)
            return string.Empty;

        if (httpContext.Request is null)
            return string.Empty;

        if (httpContext.Request.Headers is null)
            return string.Empty;

        return httpContext
            .Request
            .Headers
            .Authorization;
    }

    internal static string TryGetJwtToken
    (
        this HttpContext httpContext
    )
    {
        var authorizationHeader = httpContext.GetToken();

        if (string.IsNullOrEmpty(authorizationHeader))
            return string.Empty;

        if (!authorizationHeader.StartsWith("Bearer "))
            return string.Empty;

        return authorizationHeader["Bearer ".Length..]
        .Trim();
    }
}