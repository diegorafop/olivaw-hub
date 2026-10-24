using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PlanFeature.Queries;
public sealed class ReadCustomerPlanByFilterQuery
{
    public Guid CustomerExternalId { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
    public int BeforeId { get; set; }
}
public sealed class ReadCustomerPlanByFilterResponse
{
    public List<CustomerPlanItem> Items { get; set; } = new();
    public int? NextBeforeId { get; set; }
    public CustomerPlanItem? Current { get; set; }
}
public sealed class CustomerPlanItem
{
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal? MemoryLimitGiB { get; set; }
    public decimal? StorageLimitGiB { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public string ApprovalReference { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
}
