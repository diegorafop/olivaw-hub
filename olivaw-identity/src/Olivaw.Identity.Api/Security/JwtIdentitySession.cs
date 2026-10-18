using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Olivaw.Identity.Contracts;

namespace Olivaw.Identity.Api.Security;

public sealed class JwtIdentitySession(IHttpContextAccessor accessor)
{
    public IdentityAccess? Access
    {
        get
        {
            var principal = accessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true) return null;
            if (!Guid.TryParse(principal.FindFirstValue("sub"), out var id)) return null;
            return new IdentityAccess
            {
                UserProfileId = id,
                Name = principal.FindFirstValue("name") ?? string.Empty,
                IsRoot = principal.HasClaim("root", "true"),
                IsServiceAccount = principal.HasClaim("service", "true"),
                Permissions = principal.FindAll("permission").Select(claim => claim.Value).Distinct(StringComparer.Ordinal).ToArray()
            };
        }
    }
}
