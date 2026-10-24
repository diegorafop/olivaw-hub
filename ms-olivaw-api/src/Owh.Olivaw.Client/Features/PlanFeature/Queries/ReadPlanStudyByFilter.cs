using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PlanFeature.Queries;
public sealed class ReadPlanStudyByFilterQuery { public int BeforeId { get; set; } }
public sealed class ReadPlanStudyByFilterResponse { public List<PlanStudyItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class PlanStudyItem
{
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IncludedFeatures { get; set; } = string.Empty;
    public decimal? StorageLimitGB { get; set; }
    public decimal? MemoryLimitMiB { get; set; }
    public decimal SupportHours { get; set; }
    public decimal HourlyCost { get; set; }
    public decimal ReservePercent { get; set; }
    public decimal MarginPercent { get; set; }
    public decimal KnownMonthlyCost { get; set; }
    public decimal? SuggestedMonthlyPrice { get; set; }
    public decimal UnallocatedMonthlyCost { get; set; }
    public int MissingCosts { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PlanStudyCostItem> Costs { get; set; } = new();
}
public sealed class PlanStudyCostItem
{
    public Guid CostReferenceExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? MonthlyCost { get; set; }
    public decimal AllocationPercent { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime EffectiveAt { get; set; }
}
