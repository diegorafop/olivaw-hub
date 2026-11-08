using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.DeliveryNoteFeature.Queries;
public sealed class ReadDeliveryNoteHistoryQuery { public Guid ExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadDeliveryNoteHistoryResponse { public List<DeliveryNoteHistoryItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class DeliveryNoteHistoryItem
{
    public long Revision { get; set; } public DateTime CreatedAt { get; set; } public string AuthorName { get; set; } = string.Empty; public DateTime? PublishedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Category { get; set; } = "feature";
    public string Status { get; set; } = "draft";
    public string ApprovalReference { get; set; } = string.Empty;
    public string ServiceLabel { get; set; } = string.Empty;
    public DateTime? DeliveredAt { get; set; }
}
