using System;
namespace Owh.Olivaw.Client.Features.DeliveryNoteFeature.Commands;
public sealed class UpdateDeliveryNoteCommand
{
    public long Revision { get; set; }
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
public sealed class UpdateDeliveryNoteResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
