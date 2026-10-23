using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class ServiceDeploymentChange : Entity
{
    public int ServiceDeploymentId { get; private set; }
    public ServiceDeployment ServiceDeployment { get; private set; } = null!;
    public long Revision { get; private set; }
    public string ActorName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string Endpoint { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    private ServiceDeploymentChange() { }
    internal static ServiceDeploymentChange Create(ServiceDeployment deployment, Guid actor, string actorName, DateTime now) => new()
    {
        ExternalId = Guid.NewGuid(), ServiceDeployment = deployment, ServiceDeploymentId = deployment.Id,
        Revision = deployment.Revision, ActorUserId = actor, ActorName = actorName, CreatedAt = now,
        Name = deployment.Name, Location = deployment.Location, Endpoint = deployment.Endpoint, Status = deployment.Status
    };
}
