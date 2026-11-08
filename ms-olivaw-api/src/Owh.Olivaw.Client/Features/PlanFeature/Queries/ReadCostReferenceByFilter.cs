using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PlanFeature.Queries;
public sealed class ReadCostReferenceByFilterQuery
{
    public string? Search { get; set; }
    public int BeforeId { get; set; }
}
public sealed class ReadCostReferenceByFilterResponse
{
    public List<CostReferenceItem> Items { get; set; } = new();
    public int? NextBeforeId { get; set; }
}
public sealed class CostReferenceItem
{
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? MonthlyCost { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime EffectiveAt { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
