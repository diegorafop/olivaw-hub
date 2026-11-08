namespace Owh.Olivaw.Client.Features.LeadFeature.Commands;

public sealed class DeleteLeadCommand
{
    public long Revision { get; set; }
    public long FollowUpRevision { get; set; }
}
public sealed class DeleteLeadResponse
{
    public bool Conflict { get; set; }
    public bool AlreadyDeleted { get; set; }
}
