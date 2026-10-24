using System;
using System.Collections.Generic;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
namespace Owh.Olivaw.Client.Features.PlanFeature.Commands;
public sealed class CreatePlanStudyCommand
{
    public string Name { get; set; } = string.Empty;
    public string IncludedFeatures { get; set; } = string.Empty;
    public decimal? StorageLimitGB { get; set; }
    public decimal? MemoryLimitMiB { get; set; }
    public List<PlanEstimateLine> Lines { get; set; } = new();
    public decimal SupportHours { get; set; }
    public decimal HourlyCost { get; set; }
    public decimal ReservePercent { get; set; }
    public decimal MarginPercent { get; set; }
}
public sealed class CreatePlanStudyResponse { public Guid ExternalId { get; set; } }
