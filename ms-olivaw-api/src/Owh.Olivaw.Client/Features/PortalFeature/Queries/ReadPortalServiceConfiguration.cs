using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PortalFeature.Queries;
public sealed class ReadPortalServiceConfigurationQuery { public Guid CustomerExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadPortalServiceConfigurationResponse { public List<PortalServiceConfigurationItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class PortalServiceConfigurationItem { public Guid ExternalId { get; set; } public Guid ServiceExternalId { get; set; } public string ServiceName { get; set; } = string.Empty; public Guid? HealthResourceExternalId { get; set; } public string Label { get; set; } = string.Empty; public string PublicUrl { get; set; } = string.Empty; public bool IsActive { get; set; } public long Revision { get; set; } }
