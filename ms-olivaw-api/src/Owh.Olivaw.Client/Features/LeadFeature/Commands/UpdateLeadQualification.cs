namespace Owh.Olivaw.Client.Features.LeadFeature.Commands;

public sealed class UpdateLeadQualificationCommand
{
    public long Revision { get; set; }
    public LeadQualification Qualification { get; set; } = new();
}
public sealed class LeadQualification
{
    public LeadWebsiteAssessment? WebsiteAssessment { get; set; }
    public string CommercialObservation { get; set; } = string.Empty;
    public string RestrictedAreaUrl { get; set; } = string.Empty;
    public int SchemaVersion { get; set; } = 1;
    public string Segment { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
    public LeadPublicEmail[] PublicEmails { get; set; } = [];
    public string ContactPageUrl { get; set; } = string.Empty;
    public string ContactFormStatus { get; set; } = "not-checked";
    public DateTimeOffset? ContactCheckedAt { get; set; }
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
public sealed class LeadPublicEmail
{
    public string Email { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
}
public sealed class UpdateLeadQualificationResponse
{
    public bool Conflict { get; set; }
    public long Revision { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class LeadWebsiteAssessment
{
    public int SchemaVersion { get; set; } = 1;
    public DateTimeOffset AssessedAt { get; set; }
    public string Basis { get; set; } = "html";
    public string Strategy { get; set; } = "discovery";
    public Dictionary<string, LeadWebsiteMetric> Metrics { get; set; } = new();
}
public sealed class LeadWebsiteMetric
{
    public int? Score { get; set; }
    public string Evidence { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string Confidence { get; set; } = "unknown";
}
