using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.UsageReadingFeature.Queries;
public sealed class ReadUsageReadingByFilterQuery { public Guid ResourceExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadUsageReadingByFilterResponse
{
    public List<UsageReadingItem> Items { get; set; } = new();
    public int? NextBeforeId { get; set; }
}
public sealed class UsageReadingItem
{
    public Guid ExternalId { get; set; }
    public DateTime ObservedAt { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Metric { get; set; } = "storage-gib";
    public decimal Value { get; set; } = 0;
    public string Quality { get; set; } = "manual";
}
