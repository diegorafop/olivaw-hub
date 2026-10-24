using System;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Commands;
public sealed class UpdateDeploymentResourceCommand { public long Revision { get; set; } }
public sealed class UpdateDeploymentResourceResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
