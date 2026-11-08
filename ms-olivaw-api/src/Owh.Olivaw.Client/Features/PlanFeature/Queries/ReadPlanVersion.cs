using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PlanFeature.Queries;
public sealed class ReadPlanVersionByFilterQuery { public string PlanKey { get; set; } = string.Empty; public int BeforeId { get; set; } }
public sealed class ReadPlanVersionByFilterResponse { public List<PlanVersionItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } public PlanVersionItem? Current { get; set; } }
public sealed class PlanVersionItem
{
    public Guid ExternalId { get; set; }
    public string PlanKey { get; set; } = string.Empty;
    public int Version { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal? StorageLimitGiB { get; set; }
    public decimal? MemoryLimitMiB { get; set; }
    public decimal? CpuLimit { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public string ApprovalReference { get; set; } = string.Empty;
    public DateTime EffectiveAt { get; set; }
    public Guid? PlanStudyExternalId { get; set; }
    public decimal? EstimatedMonthlyCost { get; set; }
    public decimal? SuggestedMonthlyPrice { get; set; }
}
