using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Owh.OlivawCore.Tests.TestSupport;

internal static class TokenFactory
{
    public static string Create
    (
        DateTime from,
        DateTime to,
        params Claim[] claims
    )
    {
        return new JwtSecurityTokenHandler().WriteToken
        (
            new JwtSecurityToken
            (
                issuer: "test",
                audience: "test",
                claims: claims,
                notBefore: from,
                expires: to
            )
        );
    }
}