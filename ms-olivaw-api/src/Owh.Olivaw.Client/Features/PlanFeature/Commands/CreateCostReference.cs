using System;
namespace Owh.Olivaw.Client.Features.PlanFeature.Commands;
public sealed class CreateCostReferenceCommand
{
    public string Name { get; set; } = string.Empty;
    public decimal? MonthlyCost { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime EffectiveAt { get; set; }
}
public sealed class CreateCostReferenceResponse { public Guid ExternalId { get; set; } }
