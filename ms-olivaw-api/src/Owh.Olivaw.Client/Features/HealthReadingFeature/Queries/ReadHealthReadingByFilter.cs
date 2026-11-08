using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.HealthReadingFeature.Queries;
public sealed class ReadHealthReadingByFilterQuery { public Guid ResourceExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadHealthReadingByFilterResponse
{
    public List<HealthReadingItem> Items { get; set; } = new();
    public int? NextBeforeId { get; set; }
    public HealthReadingItem? Latest { get; set; }
    public string State { get; set; } = "unmonitored";
}
public sealed class HealthReadingItem
{
    public Guid ExternalId { get; set; }
    public DateTime ObservedAt { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Outcome { get; set; } = "source-error";
    public int MaxAgeMinutes { get; set; } = 15;
}
