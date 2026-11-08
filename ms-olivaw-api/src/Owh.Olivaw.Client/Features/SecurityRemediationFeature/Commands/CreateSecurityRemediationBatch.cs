namespace Owh.Olivaw.Client.Features.SecurityRemediationFeature.Commands;
public sealed class CreateSecurityRemediationBatchCommand
{
    public Guid ReportExternalId { get; set; }
}
public sealed class CreateSecurityRemediationBatchResponse
{
    public int AuthorizedCount { get; set; }
    public int InterventionCount { get; set; }
    public int AlreadyHandledCount { get; set; }
}
