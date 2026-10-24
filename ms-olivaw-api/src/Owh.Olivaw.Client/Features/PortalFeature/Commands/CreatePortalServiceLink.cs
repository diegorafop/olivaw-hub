using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class CreatePortalServiceLinkCommand { public Guid CustomerExternalId { get; set; } public Guid ServiceExternalId { get; set; } public Guid? HealthResourceExternalId { get; set; } public string Label { get; set; } = string.Empty; public string PublicUrl { get; set; } = string.Empty; }
public sealed class CreatePortalServiceLinkResponse { public Guid ExternalId { get; set; } }
