namespace Owh.Olivaw.Client.Features.SecurityReportFeature.Queries;

public sealed class ReadSecurityReportByFilterQuery
{
    public int BeforeId { get; set; }
    public string? HostId { get; set; }
}

public sealed class ReadSecurityReportByFilterResponse
{
    public List<SecurityReportItem> Items { get; set; } = new();
    public int? NextBeforeId { get; set; }
}

public sealed class SecurityReportItem
{
    public Guid ExternalId { get; set; }
    public Guid RunId { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string HostId { get; set; } = string.Empty;
    public string ExecutionStatus { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RawData { get; set; } = string.Empty;
}
