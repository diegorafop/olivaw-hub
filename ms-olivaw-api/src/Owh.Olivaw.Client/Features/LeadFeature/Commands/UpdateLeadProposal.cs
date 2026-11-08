namespace Owh.Olivaw.Client.Features.LeadFeature.Commands;

public sealed class UpdateLeadProposalCommand
{
    public long Revision { get; set; }
    public LeadProposal Proposal { get; set; } = new();
}
public sealed class UpdateLeadProposalResponse
{
    public long Revision { get; set; }
    public bool Conflict { get; set; }
}
public sealed class LeadProposal
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string Platform { get; set; } = string.Empty;
    public string RequirementsAnalysis { get; set; } = string.Empty;
    public string TechnicalApproach { get; set; } = string.Empty;
    public string Assumptions { get; set; } = string.Empty;
    public string ExternalCosts { get; set; } = string.Empty;
    public string Deadline { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;
    public string Continuity { get; set; } = string.Empty;
    public decimal ReservePercent { get; set; }
    public decimal DiscountPercent { get; set; }
    public int Installments { get; set; } = 1;
    public LeadProposalRow[] Rows { get; set; } = [];
}
public sealed class LeadProposalRow
{
    public string Description { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Solution { get; set; } = "integration";
    public decimal Hours { get; set; }
    public decimal HourlyRate { get; set; }
    public bool Optional { get; set; }
}
