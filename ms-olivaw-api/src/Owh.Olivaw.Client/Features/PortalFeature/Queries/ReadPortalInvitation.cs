using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Queries;
public sealed class ReadPortalInvitationQuery { public string Token { get; set; } = string.Empty; }
public sealed class ReadPortalInvitationResponse { public string Email { get; set; } = string.Empty; public string Business { get; set; } = string.Empty; public bool ExistingAccount { get; set; } public DateTime ExpiresAt { get; set; } }
