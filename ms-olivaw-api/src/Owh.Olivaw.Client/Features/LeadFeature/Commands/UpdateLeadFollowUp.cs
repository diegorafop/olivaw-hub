namespace Owh.Olivaw.Client.Features.LeadFeature.Commands;

public sealed class UpdateLeadFollowUpCommand
{
    public long Revision { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}
public sealed class UpdateLeadFollowUpResponse
{
    public long Revision { get; set; }
}
