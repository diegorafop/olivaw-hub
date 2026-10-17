using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mps.Auth.Api.Helpers;

public static class JwtSecurityTokenHelper
{
    public static IEnumerable<Claim> GetClaimsByIdToken
    (
        string idToken
    )
    {
        if (string.IsNullOrEmpty(idToken))
            return [];

        return new JwtSecurityTokenHandler()
            .ReadJwtToken(idToken)
            .Claims;
    }
}