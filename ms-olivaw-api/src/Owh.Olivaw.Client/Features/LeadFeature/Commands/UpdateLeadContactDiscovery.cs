namespace Owh.Olivaw.Client.Features.LeadFeature.Commands;

public sealed class UpdateLeadContactDiscoveryCommand
{
    public long Revision { get; set; }
    public long FollowUpRevision { get; set; }
    public bool ResearchCompleted { get; set; }
    public LeadQualification Contacts { get; set; } = new();
}
public sealed class UpdateLeadContactDiscoveryResponse
{
    public bool Conflict { get; set; }
    public long Revision { get; set; }
    public long FollowUpRevision { get; set; }
    public string Status { get; set; } = string.Empty;
}
