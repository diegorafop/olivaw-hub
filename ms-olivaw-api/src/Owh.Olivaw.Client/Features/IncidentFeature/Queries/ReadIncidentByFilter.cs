using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.IncidentFeature.Queries;
public sealed class ReadIncidentByFilterQuery
{
    public Guid? ResourceExternalId { get; set; }
    public string? Status { get; set; }
    public int BeforeId { get; set; }
}
public sealed class ReadIncidentByFilterResponse { public List<IncidentItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class IncidentItem
{
    public Guid ExternalId { get; set; }
    public Guid ResourceExternalId { get; set; }
    public long Revision { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public string Severity { get; set; } = "attention";
    public string Status { get; set; } = "detected";
}
