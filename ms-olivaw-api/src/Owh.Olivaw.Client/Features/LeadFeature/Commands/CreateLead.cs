namespace Owh.Olivaw.Client.Features.LeadFeature.Commands;

public sealed class CreateLeadCommand
{
    public Guid ExternalId { get; set; }
    public string Initiative { get; set; } = "outbound";
    public string Origin { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string Business { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
}
public sealed class CreateLeadResponse
{
    public Guid ExternalId { get; set; }
    public bool Conflict { get; set; }
}
public sealed class LeadAcquisition
{
    public string Initiative { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
}
