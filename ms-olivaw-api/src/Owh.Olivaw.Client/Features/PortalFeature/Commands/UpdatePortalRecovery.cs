using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class UpdatePortalRecoveryCommand { public long Revision { get; set; } public string Reason { get; set; } = string.Empty; }
public sealed class UpdatePortalRecoveryResponse { public Guid ExternalId { get; set; } public string Token { get; set; } = string.Empty; public DateTime ExpiresAt { get; set; } }
