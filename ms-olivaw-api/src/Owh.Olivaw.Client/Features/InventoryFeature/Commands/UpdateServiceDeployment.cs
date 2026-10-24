using System;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Commands;
public sealed class UpdateServiceDeploymentCommand
{
    public long Revision { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Status { get; set; } = "planned";
}
public sealed class UpdateServiceDeploymentResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
