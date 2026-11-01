using System;
namespace Owh.Olivaw.Client.Features.PlanFeature.Commands;
public sealed class CreatePlanVersionCommand
{
    public Guid PublicationKey { get; set; }
    public string PlanKey { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal? StorageLimitGiB { get; set; }
    public decimal? MemoryLimitMiB { get; set; }
    public decimal? CpuLimit { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public string ApprovalReference { get; set; } = string.Empty;
    public DateTime EffectiveAt { get; set; }
    public Guid? PlanStudyExternalId { get; set; }
}
public sealed class CreatePlanVersionResponse { public Guid ExternalId { get; set; } public int Version { get; set; } }
