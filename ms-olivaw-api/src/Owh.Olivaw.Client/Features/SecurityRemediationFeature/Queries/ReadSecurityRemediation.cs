namespace Owh.Olivaw.Client.Features.SecurityRemediationFeature.Queries;
public sealed class ReadSecurityRemediationQuery
{
    public Guid ReportExternalId { get; set; }
}
public sealed class ReadSecurityRemediationResponse
{
    public List<SecurityRemediationItem> Items { get; set; } = new();
}
public sealed class ReadSecurityRemediationItemQuery
{
    public string HostId { get; set; } = string.Empty;
}
public sealed class ReadSecurityRemediationQueueResponse
{
    public List<SecurityRemediationItem> Items { get; set; } = new();
}
public sealed class SecurityRemediationItem
{
    public Guid ExternalId { get; set; }
    public int FindingIndex { get; set; }
    public string Status { get; set; } = "review";
    public string Message { get; set; } = string.Empty;
    public string HostId { get; set; } = string.Empty;
    public string PlanJson { get; set; } = string.Empty;
    public bool Supported { get; set; }
    public string PlanTitle { get; set; } = string.Empty;
    public DateTime? ApprovalExpiresAt { get; set; }
}
