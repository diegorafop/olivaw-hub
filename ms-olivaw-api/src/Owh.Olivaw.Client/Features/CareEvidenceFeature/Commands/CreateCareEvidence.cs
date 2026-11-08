using System;
namespace Owh.Olivaw.Client.Features.CareEvidenceFeature.Commands;
public sealed class CreateCareEvidenceCommand
{
    public Guid ResourceExternalId { get; set; }
    public Guid SampleKey { get; set; }
    public DateTime ObservedAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Kind { get; set; } = "backup";
    public string Outcome { get; set; } = "unknown";
    public string ReferenceUrl { get; set; } = string.Empty;
}
public sealed class CreateCareEvidenceResponse { public Guid ExternalId { get; set; } }
