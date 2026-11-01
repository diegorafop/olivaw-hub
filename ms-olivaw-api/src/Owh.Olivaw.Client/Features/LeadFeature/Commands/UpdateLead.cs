namespace Owh.Olivaw.Client.Features.LeadFeature.Commands;

public sealed class UpdateLeadCommand
{
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public LeadAcquisition? Acquisition { get; set; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public LeadProposal? Proposal { get; set; }
    public int SchemaVersion { get; set; }
    public long Revision { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public string Entry { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public LeadProject? Project { get; set; }
    public string[] Excluded { get; set; } = [];
    public string Requirements { get; set; } = string.Empty;
    public string Business { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Whatsapp { get; set; }
    public string? Plan { get; set; }
    public LeadJourneyEvent[] Events { get; set; } = [];
}
public sealed class LeadProject
{
    public string Source { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string[] Choices { get; set; } = [];
    public string Context { get; set; } = string.Empty;
    public string? PresetId { get; set; }
    public string[] BlockIds { get; set; } = [];
}
public sealed class LeadJourneyEvent
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTimeOffset At { get; set; }
}
public sealed class UpdateLeadResponse
{
    public Guid ExternalId { get; set; }
    public long Revision { get; set; }
    public DateTime SavedAt { get; set; }
}
