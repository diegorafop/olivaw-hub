using Microsoft.AspNetCore.Authorization;

namespace Olivaw.Identity.Api.Security;

public sealed class JwtPermissionHandler(JwtIdentitySession session) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true) return Task.CompletedTask;
        var access = session.Access;
        if (access is null) return Task.CompletedTask;
        if (access.IsRoot)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        if (access.Permissions.Contains(requirement.Key, StringComparer.Ordinal)) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
