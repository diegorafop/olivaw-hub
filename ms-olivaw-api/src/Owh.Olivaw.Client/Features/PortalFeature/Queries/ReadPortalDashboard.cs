using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.PortalFeature.Queries;
public sealed class ReadPortalDashboardQuery { public Guid CustomerExternalId { get; set; } public int BeforeId { get; set; } }
public sealed class ReadPortalDashboardResponse
{
    public Guid CustomerExternalId { get; set; } public string Business { get; set; } = string.Empty;
    public bool Preview { get; set; } public bool CanRequest { get; set; } public bool CanReadBilling { get; set; }
    public List<PortalServiceItem> Services { get; set; } = new(); public int? NextBeforeId { get; set; }
    public PortalPlanItem? CurrentPlan { get; set; }
}
public sealed class PortalServiceItem { public Guid ServiceExternalId { get; set; } public Guid ExternalId { get; set; } public string Name { get; set; } = string.Empty; public string PublicUrl { get; set; } = string.Empty; public string State { get; set; } = "unknown"; public DateTime? ObservedAt { get; set; } }
public sealed class PortalPlanItem { public string Name { get; set; } = string.Empty; public decimal MonthlyPrice { get; set; } public string Currency { get; set; } = "BRL"; public decimal? StorageLimitGiB { get; set; } public decimal? MemoryLimitGiB { get; set; } public string Conditions { get; set; } = string.Empty; public DateTime StartsAt { get; set; } }
