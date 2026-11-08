using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
public sealed class ReadSupportRequestHistoryQuery { public Guid ExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadSupportRequestHistoryResponse { public List<SupportRequestHistoryItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class SupportRequestHistoryItem
{
    public long Revision { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Requester { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
    public string PublicSummary { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public string Category { get; set; } = "question";
    public string Status { get; set; } = "received";
}
