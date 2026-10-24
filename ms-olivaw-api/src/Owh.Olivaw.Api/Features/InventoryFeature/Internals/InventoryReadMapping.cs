using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Repositories;
namespace Owh.Olivaw.Api.Features.InventoryFeature.Internals;
internal static class InventoryReadMapping
{
    internal static InventoryOriginItem Origin(InventoryReadOrigin item) => new()
    {
        Kind = item.Kind, FromResourceExternalId = item.FromResourceExternalId, FromResourceName = item.FromResourceName,
        DeploymentExternalId = item.DeploymentExternalId, ServiceName = item.ServiceName, Environment = item.Environment,
        RelationshipKind = item.RelationshipKind, Purpose = item.Purpose
    };
}
