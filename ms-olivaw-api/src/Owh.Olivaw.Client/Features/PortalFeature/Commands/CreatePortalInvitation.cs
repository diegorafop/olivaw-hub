using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class CreatePortalInvitationCommand
{
    public Guid CustomerExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "requester";
}
public sealed class CreatePortalInvitationResponse { public Guid ExternalId { get; set; } public string Token { get; set; } = string.Empty; public DateTime ExpiresAt { get; set; } }
