using System;
using System.Collections.Generic;

namespace Owh.Olivaw.Client.Features.LeadFeature.Queries;

public sealed class ReadLeadHistoryQuery
{
    public Guid ExternalId { get; set; }
    public long? BeforeRevision { get; set; }
}
public sealed class ReadLeadHistoryResponse
{
    public List<LeadHistoryItem> Items { get; set; } = new();
    public long? NextBeforeRevision { get; set; }
}
public sealed class LeadHistoryItem
{
    public Guid ExternalId { get; set; }
    public long Revision { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
