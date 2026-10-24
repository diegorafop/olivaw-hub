using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Owh.OlivawCore.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Xml.Serialization;

namespace Owh.OlivawCore.Api.Extensions.Https;

public static class HttpContextExtensions
{
    public static string GetValue
    (
        this IEnumerable<Claim> claims,
        string? claimType
    )
    {
        if (string.IsNullOrEmpty(claimType))
            return string.Empty;

        var item = claims.FirstOrDefault(claim => claim
            .Type
            .Equals(claimType));

        if (item is null)
            return string.Empty;

        return item.Value;
    }

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
            .Headers.Authorization;
    }

    public static Guid GetUserProfileId
    (
        this HttpContext httpContext,
        IConfiguration configuration,
        Guid? usuarioId
    )
    {
        if (UserProfileIdHasValue(usuarioId, out Guid usuarioIdValue))
            return usuarioIdValue;

        if (httpContext is null)
            return Guid.Empty;

        var value = configuration.GetValue<string>
        (
            "authentication:oidc:standard-claims:sub"
        );

        return Guid.Parse
        (
            GetValue
            (
                httpContext,
                value ?? string.Empty
            )
        );
    }

    public static Guid GetUserProfileId
    (
        this HttpContext httpContext,
        IConfiguration configuration
    )
    {
        if (httpContext == null)
            return Guid.Empty;

        var value = configuration.GetValue<string>
        (
            "authentication:oidc:standard-claims:sub"
        );

        return Guid.Parse
        (
            GetValue
            (
                httpContext,
                value ?? string.Empty
            )
        );
    }

    private static bool UserProfileIdHasValue
    (
        Guid? usuarioId,
        out Guid value
    )
    {
        value = Guid.Empty;

        if (!usuarioId.HasValue)
            return false;

        value = usuarioId.Value;

        return !usuarioId.Equals(Guid.Empty);
    }

    internal static string GetValue
    (
        this HttpContext httpContext,
        string claimType
    )
    {
        if (httpContext is null)
            return string.Empty;

        var claim = httpContext.GetClaims()
            .FirstOrDefault(claim => claim.Type == claimType);

        if (claim is null)
            return string.Empty;

        return claim.Value;
    }

    public static IEnumerable<Claim> GetClaims
    (
        this HttpContext httpContext
    )
    {
        string jwtToken = GetToken(httpContext);

        if (string.IsNullOrEmpty(jwtToken))
            return [];

        SecurityToken securityToken = ReadToken(jwtToken);

        return ((JwtSecurityToken)securityToken).Claims;
    }

    public static T DeserializeXml<T>
    (
        this HttpContext httpContext
    )
    {
        if (httpContext == null)
            return default;

        var content = GetContent(httpContext);
        MemoryStream memoryStream = new(content);
        XmlSerializer xmlSerializer = new(typeof(T));

        var response = xmlSerializer.Deserialize(memoryStream);

        if (response == null)
            return default;

        return (T)response;
    }

    private static byte[] GetContent
    (
        HttpContext httpContext
    )
    {
        StreamReader reader = new
        (
            httpContext.Request.Body,
            Encoding.UTF8
        );

        string content = reader
            .ReadToEndAsync()
            .GetAwaiter()
            .GetResult();

        return Encoding
            .UTF8
            .GetBytes(content);
    }

    public static bool JwtTokenIsValid
    (
        this AuthenticateUserCommandResponse? response
    )
    {
        if (response is null)
            return false;

        return response
            .Token
            .JwtTokenIsValid();
    }

    public static bool JwtTokenIsValid
    (
        this string jwtToken
    )
    {
        if (string.IsNullOrEmpty(jwtToken))
            return false;

        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(jwtToken))
            return false;

        SecurityToken? securityToken = handler.GetSecurityToken(jwtToken);

        if (securityToken is null)
            return false;

        if (securityToken.ValidTo == DateTime.MinValue)
            return false;

        if (securityToken.ValidFrom > DateTime.UtcNow)
            return false;

        if (securityToken.ValidToMinusOne() < DateTime.UtcNow)
            return false;

        return true;
    }

    private static DateTime ValidToMinusOne
    (
        this SecurityToken securityToken
    )
    {
        return securityToken
            .ValidTo
            .AddMinutes(-1);
    }

    private static SecurityToken ReadToken
    (
        string jwtToken
    )
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler
            .ReadToken
            (
                jwtToken
                    .Replace
                    (
                        "Bearer",
                        string.Empty
                    )
                    .Trim()
            );
    }

    private static SecurityToken? GetSecurityToken
    (
        this JwtSecurityTokenHandler handler,
        string jwtToken
    )
    {
        if (string.IsNullOrEmpty(jwtToken))
            return null;

        try
        {
            var value = jwtToken
                .Replace
                (
                    "Bearer",
                    string.Empty
                )
                .Trim();

            return handler.ReadToken(value);
        }
        catch
        {
            return null;
        }
    }

    public static bool ShouldMapRequestMetrics
    (
        this HttpContext context
    )
    {
        if (context.RequestPathContains("/openapi"))
            return false;

        if (context.RequestPathContains("/swagger"))
            return false;

        if (context.RequestPathContains("/health"))
            return false;

        if (context.RequestPathContains("/metrics"))
            return false;

        return true;
    }

    public static bool RequestPathContains
    (
        this HttpContext? context,
        string value
    )
    {
        if (context is null)
            return false;

        if (context.Request is null)
            return false;

        var path = context
            .Request
            .Path;

        if (!path.HasValue)
            return false;

        if (string.IsNullOrEmpty(path.Value))
            return false;

        return path
            .Value
            .Contains
            (
                value,
                StringComparison.OrdinalIgnoreCase
            );
    }
}