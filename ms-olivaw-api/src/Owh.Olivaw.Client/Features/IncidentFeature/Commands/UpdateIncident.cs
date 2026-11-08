using System;
namespace Owh.Olivaw.Client.Features.IncidentFeature.Commands;
public sealed class UpdateIncidentCommand
{
    public long Revision { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string NextAction { get; set; } = string.Empty;
    public string InternalNote { get; set; } = string.Empty;
    public string Severity { get; set; } = "attention";
    public string Status { get; set; } = "detected";
}
public sealed class UpdateIncidentResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
