using Microsoft.AspNetCore.Authorization;
using Olivaw.Identity.Api.Security;

namespace Owh.Devops.Api.Security;

public sealed record RootAccessRequirement : IAuthorizationRequirement;

public sealed class RootAccessHandler(JwtIdentitySession session) : AuthorizationHandler<RootAccessRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RootAccessRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true) return Task.CompletedTask;
        if (session.Access?.IsRoot == true) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
