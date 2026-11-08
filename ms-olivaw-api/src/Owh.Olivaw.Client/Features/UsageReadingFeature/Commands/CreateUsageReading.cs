using System;
namespace Owh.Olivaw.Client.Features.UsageReadingFeature.Commands;
public sealed class CreateUsageReadingCommand
{
    public Guid ResourceExternalId { get; set; }
    public Guid SampleKey { get; set; }
    public DateTime ObservedAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Metric { get; set; } = "storage-gib";
    public decimal Value { get; set; } = 0;
    public string Quality { get; set; } = "manual";
}
public sealed class CreateUsageReadingResponse { public Guid ExternalId { get; set; } }
