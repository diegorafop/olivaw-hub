using Microsoft.AspNetCore.Authorization;

namespace Olivaw.Identity.Api.Security;

public sealed record PermissionRequirement(string Key) : IAuthorizationRequirement;
