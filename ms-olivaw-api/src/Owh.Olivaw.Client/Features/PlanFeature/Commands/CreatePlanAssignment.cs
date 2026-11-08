using System;
namespace Owh.Olivaw.Client.Features.PlanFeature.Commands;
public sealed class CreatePlanAssignmentCommand
{
    public Guid CustomerExternalId { get; set; }
    public Guid PlanVersionExternalId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal? AgreedMonthlyPrice { get; set; }
    public string ApprovalReference { get; set; } = string.Empty;
}
public sealed class CreatePlanAssignmentResponse { public Guid ExternalId { get; set; } }
