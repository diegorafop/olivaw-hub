using System;
namespace Owh.Olivaw.Client.Features.PlanFeature.Commands;
public sealed class CreateCustomerPlanCommand
{
    public Guid CustomerExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal? MemoryLimitGiB { get; set; }
    public decimal? StorageLimitGiB { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public string ApprovalReference { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
}
public sealed class CreateCustomerPlanResponse { public Guid ExternalId { get; set; } }
