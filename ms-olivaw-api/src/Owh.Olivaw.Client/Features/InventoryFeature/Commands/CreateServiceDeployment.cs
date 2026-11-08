using System;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Commands;
public sealed class CreateServiceDeploymentCommand
{
    public Guid ServiceExternalId { get; set; }
    public string Environment { get; set; } = "production";
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Status { get; set; } = "planned";
}
public sealed class CreateServiceDeploymentResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
