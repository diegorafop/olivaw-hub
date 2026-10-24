using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.CareEvidenceFeature.Queries;
public sealed class ReadCareEvidenceByFilterQuery { public Guid ResourceExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadCareEvidenceByFilterResponse
{
    public List<CareEvidenceItem> Items { get; set; } = new();
    public int? NextBeforeId { get; set; }
}
public sealed class CareEvidenceItem
{
    public Guid ExternalId { get; set; }
    public DateTime ObservedAt { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Kind { get; set; } = "backup";
    public string Outcome { get; set; } = "unknown";
    public string ReferenceUrl { get; set; } = string.Empty;
}
