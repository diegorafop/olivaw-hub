using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PortalFeature.Queries;
public sealed class ReadPortalAccessAuditQuery { public Guid CustomerExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadPortalAccessAuditResponse { public List<PortalAccessAuditItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class PortalAccessAuditItem { public Guid ExternalId { get; set; } public string Action { get; set; } = string.Empty; public string Details { get; set; } = string.Empty; public string ActorAuthority { get; set; } = string.Empty; public string ActorName { get; set; } = string.Empty; public DateTime CreatedAt { get; set; } }
