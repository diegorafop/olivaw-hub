using System;
namespace Owh.Olivaw.Client.Features.HealthReadingFeature.Commands;
public sealed class CreateHealthReadingCommand
{
    public Guid ResourceExternalId { get; set; }
    public Guid SampleKey { get; set; }
    public DateTime ObservedAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Outcome { get; set; } = "source-error";
    public int MaxAgeMinutes { get; set; } = 15;
}
public sealed class CreateHealthReadingResponse { public Guid ExternalId { get; set; } }
