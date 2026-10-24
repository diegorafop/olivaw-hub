using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PortalFeature.Queries;
public sealed class ReadPortalRequestsQuery { public Guid CustomerExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadPortalRequestsResponse { public List<PortalRequestItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class PortalRequestItem { public Guid ExternalId { get; set; } public string Protocol { get; set; } = string.Empty; public string Title { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string RequestType { get; set; } = string.Empty; public string PublicSummary { get; set; } = string.Empty; public Guid? ServiceExternalId { get; set; } public string ServiceLabel { get; set; } = string.Empty; public decimal? DesiredStorageGB { get; set; } public decimal? DesiredMemoryMiB { get; set; } public string DesiredPlan { get; set; } = string.Empty; public DateTime CreatedAt { get; set; } public DateTime? UpdatedAt { get; set; } public long Revision { get; set; } }
