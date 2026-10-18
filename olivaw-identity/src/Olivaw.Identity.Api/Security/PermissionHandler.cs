using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Olivaw.Identity.Api.Security;

public sealed class PermissionHandler(IIdentityStore store, IHttpContextAccessor accessor) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (!Guid.TryParse(context.User.FindFirstValue("sub"), out var id)) return;
        var token = accessor.HttpContext?.RequestAborted ?? CancellationToken.None;
        var usuario = await store.FindAsync<UserProfile>(item => item.ExternalId == id, token);
        if (usuario is null) return;
        if (!usuario.IsActive) return;
        if (context.User.FindFirstValue("stamp") != usuario.SecurityStamp.ToString()) return;
        if (usuario.IsRoot)
        {
            context.Succeed(requirement);
            return;
        }
        var permissions = await store.PermissionsAsync(usuario.Id, token);
        if (permissions.Contains(requirement.Key, StringComparer.Ordinal)) context.Succeed(requirement);
    }
}
