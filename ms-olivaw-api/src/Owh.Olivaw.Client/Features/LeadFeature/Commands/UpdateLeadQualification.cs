namespace Owh.Olivaw.Client.Features.LeadFeature.Commands;

public sealed class UpdateLeadQualificationCommand
{
    public long Revision { get; set; }
    public LeadQualification Qualification { get; set; } = new();
}
public sealed class LeadQualification
{
    public int SchemaVersion { get; set; } = 1;
    public string Segment { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
    public string ContactFormUrl { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string Presence { get; set; } = "unknown";
    public string Priority { get; set; } = "unassessed";
    public string Confidence { get; set; } = "pending";
    public string[] Tags { get; set; } = [];
    public string PortfolioReference { get; set; } = string.Empty;
    public string Facts { get; set; } = string.Empty;
    public string Positives { get; set; } = string.Empty;
    public string Observations { get; set; } = string.Empty;
    public string Questions { get; set; } = string.Empty;
    public string Approach { get; set; } = string.Empty;
    public string ApproachStatus { get; set; } = "draft";
}
public sealed class UpdateLeadQualificationResponse
{
    public bool Conflict { get; set; }
    public long Revision { get; set; }
    public DateTime UpdatedAt { get; set; }
}
