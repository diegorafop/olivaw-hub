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
        var usuario = await store.FindAsync<Usuario>(item => item.IdExterno == id, token);
        if (usuario is null) return;
        if (!usuario.Ativo) return;
        if (context.User.FindFirstValue("stamp") != usuario.VersaoSeguranca.ToString()) return;
        if (usuario.Root)
        {
            context.Succeed(requirement);
            return;
        }
        var permissions = await store.PermissionsAsync(usuario.Id, token);
        if (permissions.Contains(requirement.Key, StringComparer.Ordinal)) context.Succeed(requirement);
    }
}
