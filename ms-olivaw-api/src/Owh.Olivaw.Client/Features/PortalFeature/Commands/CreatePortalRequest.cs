using System;
namespace Owh.Olivaw.Client.Features.PortalFeature.Commands;
public sealed class CreatePortalRequestCommand
{
    public Guid CustomerExternalId { get; set; } public Guid? ServiceExternalId { get; set; } public Guid RequestKey { get; set; }
    public string Title { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public string RequestType { get; set; } = "question";
    public decimal? DesiredStorageGB { get; set; } public decimal? DesiredMemoryMiB { get; set; } public string DesiredPlan { get; set; } = string.Empty;
}
public sealed class CreatePortalRequestResponse { public Guid ExternalId { get; set; } public string Protocol { get; set; } = string.Empty; }
