using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.IncidentFeature.Queries;
public sealed class ReadIncidentHistoryQuery { public Guid ExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadIncidentHistoryResponse { public List<IncidentHistoryItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class IncidentHistoryItem
{
    public long Revision { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public string Severity { get; set; } = "attention";
    public string Status { get; set; } = "detected";
}
