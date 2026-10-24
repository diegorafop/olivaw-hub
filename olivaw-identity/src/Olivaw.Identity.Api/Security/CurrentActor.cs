using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Olivaw.Identity.Api.Services;

namespace Olivaw.Identity.Api.Security;

public sealed class CurrentActor(IHttpContextAccessor accessor)
{
    public Guid Id
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue("sub");
            if (Guid.TryParse(value, out var id)) return id;
            throw new IdentityFailure(401, "Autenticação obrigatória.");
        }
    }
}
