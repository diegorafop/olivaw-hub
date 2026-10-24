using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PlanFeature.Queries;
public sealed class ReadPlanEstimateQuery
{
    public List<PlanEstimateLine> Lines { get; set; } = new();
    public decimal SupportHours { get; set; }
    public decimal HourlyCost { get; set; }
    public decimal ReservePercent { get; set; }
    public decimal MarginPercent { get; set; }
}
public sealed class PlanEstimateLine
{
    public Guid CostReferenceExternalId { get; set; }
    public decimal AllocationPercent { get; set; }
}
public sealed class ReadPlanEstimateResponse
{
    public string Currency { get; set; } = "BRL";
    public decimal KnownMonthlyCost { get; set; }
    public decimal UnallocatedMonthlyCost { get; set; }
    public decimal? SuggestedMonthlyPrice { get; set; }
    public int MissingCosts { get; set; }
}
