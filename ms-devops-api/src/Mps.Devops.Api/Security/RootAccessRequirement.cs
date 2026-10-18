using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Olivaw.Identity.Domain;
using Olivaw.Identity.Repositories;

namespace Mps.Devops.Api.Security;

public sealed record RootAccessRequirement : IAuthorizationRequirement;

public sealed class RootAccessHandler(IIdentityStore store, IHttpContextAccessor accessor)
    : AuthorizationHandler<RootAccessRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RootAccessRequirement requirement)
    {
        if (!Guid.TryParse(context.User.FindFirstValue("sub"), out var id)) return;
        var token = accessor.HttpContext?.RequestAborted ?? CancellationToken.None;
        var user = await store.FindAsync<Usuario>(item => item.IdExterno == id, token);
        if (user is null) return;
        if (!user.Ativo) return;
        if (!user.Root) return;
        if (context.User.FindFirstValue("stamp") != user.VersaoSeguranca.ToString()) return;
        context.Succeed(requirement);
    }
}
