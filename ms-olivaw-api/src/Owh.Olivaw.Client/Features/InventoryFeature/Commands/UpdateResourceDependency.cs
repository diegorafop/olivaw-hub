using System;
namespace Owh.Olivaw.Client.Features.InventoryFeature.Commands;
public sealed class UpdateResourceDependencyCommand { public long Revision { get; set; } }
public sealed class UpdateResourceDependencyResponse { public Guid ExternalId { get; set; } public long Revision { get; set; } }
