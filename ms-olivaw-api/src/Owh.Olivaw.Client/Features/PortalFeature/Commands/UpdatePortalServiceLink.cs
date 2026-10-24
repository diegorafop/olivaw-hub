using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class UpdatePortalServiceLinkCommand { public long Revision { get; set; } public Guid? HealthResourceExternalId { get; set; } public string Label { get; set; } = string.Empty; public string PublicUrl { get; set; } = string.Empty; public bool IsActive { get; set; } = true; }
public sealed class UpdatePortalServiceLinkResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
