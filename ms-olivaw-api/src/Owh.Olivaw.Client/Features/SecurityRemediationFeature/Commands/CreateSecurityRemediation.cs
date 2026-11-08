namespace Owh.Olivaw.Client.Features.SecurityRemediationFeature.Commands;
public sealed class CreateSecurityRemediationCommand
{
    public Guid ReportExternalId { get; set; }
    public int FindingIndex { get; set; }
}
public sealed class CreateSecurityRemediationResponse
{
    public Guid ExternalId { get; set; }
    public string Status { get; set; } = string.Empty;
}
