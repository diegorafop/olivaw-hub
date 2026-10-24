namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class UpdatePortalInvitationCommand { public string Token { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public sealed class UpdatePortalInvitationResponse { public bool Accepted { get; set; } }
