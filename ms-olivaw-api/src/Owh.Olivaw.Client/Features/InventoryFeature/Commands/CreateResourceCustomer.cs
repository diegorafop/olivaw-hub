using System;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Commands;
public sealed class CreateResourceCustomerCommand
{
    public Guid ResourceExternalId { get; set; }
    public Guid TargetExternalId { get; set; }
    public string Purpose { get; set; } = string.Empty;
}
public sealed class CreateResourceCustomerResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
