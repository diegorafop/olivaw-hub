using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class CreatePortalUsageSampleCommand
{
    public Guid CustomerExternalId { get; set; }
    public Guid SampleKey { get; set; }
    public DateTime Day { get; set; }
    public decimal? StorageUsedMiB { get; set; }
    public long? PageViews { get; set; }
    public string Source { get; set; } = string.Empty;
}
public sealed class CreatePortalUsageSampleResponse { public Guid ExternalId { get; set; } }
