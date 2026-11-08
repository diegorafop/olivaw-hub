namespace Owh.Olivaw.Client.Features.LeadFeature.Commands;

public sealed class UpdateLeadCommunicationCommand
{
    public long Revision { get; set; }
    public long FollowUpRevision { get; set; }
    public LeadCommunicationEvent Event { get; set; } = new();
}
public sealed class LeadCommunicationEvent
{
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? InterestSignal { get; set; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? InterestEvidence { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string IntentId { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string BodyPreview { get; set; } = string.Empty;
    public string GmailMessageId { get; set; } = string.Empty;
    public string ThreadId { get; set; } = string.Empty;
    public string ReplyMessageId { get; set; } = string.Empty;
}
public sealed class UpdateLeadCommunicationResponse
{
    public bool Conflict { get; set; }
    public bool Repeated { get; set; }
    public long Revision { get; set; }
    public long FollowUpRevision { get; set; }
}
