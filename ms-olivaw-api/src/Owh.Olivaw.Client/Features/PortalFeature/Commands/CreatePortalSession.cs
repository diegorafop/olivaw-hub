using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class CreatePortalSessionCommand { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public sealed class CreatePortalSessionResponse { public string AccessToken { get; set; } = string.Empty; public DateTime ExpiresAt { get; set; } }
