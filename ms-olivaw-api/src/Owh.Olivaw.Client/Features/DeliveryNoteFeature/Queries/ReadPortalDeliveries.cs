using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
public sealed class ReadPortalDeliveriesQuery
{
    public Guid CustomerExternalId { get; set; } public Guid? ServiceExternalId { get; set; } public string? Category { get; set; }
    public DateTime? FromAt { get; set; } public DateTime? ToAt { get; set; } public DateTime? BeforeAt { get; set; } public int BeforeId { get; set; }
}
public sealed class ReadPortalDeliveriesResponse { public List<PortalDeliveryItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } public DateTime? NextBeforeAt { get; set; } }
public sealed class PortalDeliveryItem { public Guid ExternalId { get; set; } public string Title { get; set; } = string.Empty; public string Summary { get; set; } = string.Empty; public string Version { get; set; } = string.Empty; public string Category { get; set; } = string.Empty; public string ServiceLabel { get; set; } = string.Empty; public Guid? ServiceExternalId { get; set; } public Guid? RequestExternalId { get; set; } public DateTime DeliveredAt { get; set; } public DateTime PublishedAt { get; set; } public DateTime? UpdatedAt { get; set; } public long Revision { get; set; } }
