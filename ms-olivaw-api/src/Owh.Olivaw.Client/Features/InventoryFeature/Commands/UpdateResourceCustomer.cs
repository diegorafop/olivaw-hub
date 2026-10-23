using System;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Commands;
public sealed class UpdateResourceCustomerCommand { public long Revision { get; set; } }
public sealed class UpdateResourceCustomerResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
