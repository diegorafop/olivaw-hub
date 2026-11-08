using System;
using System.Collections.Generic;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;

public sealed class ServiceDeployment : Entity
{
    public int ManagedServiceId { get; private set; }
    public ManagedService ManagedService { get; private set; } = null!;
    public string Environment { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string Endpoint { get; private set; } = string.Empty;
    public string Status { get; private set; } = "planned";
    public long Revision { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public ICollection<ServiceDeploymentChange> Changes { get; private set; } = new List<ServiceDeploymentChange>();
    private ServiceDeployment() { }
    public static bool IsEnvironment(string value) => value is "development" or "quality" or "release" or "staging" or "production";
    public static bool IsStatus(string value) => value is "planned" or "active" or "closed";
    public static ServiceDeployment Create(ManagedService service, string environment, string name, string location, string endpoint, string status, Guid actor, string actorName, DateTime now)
    {
        if (!IsEnvironment(environment)) throw new ArgumentException("Ambiente inválido.");
        if (service.Status == "closed") throw new ArgumentException("Serviço encerrado.");
        var deployment = new ServiceDeployment { ExternalId = Guid.NewGuid(), ManagedService = service, ManagedServiceId = service.Id, Environment = environment, CreatedAt = now };
        deployment.Changes.Add(deployment.Apply(name, location, endpoint, status, actor, actorName, now));
        return deployment;
    }
    public ServiceDeploymentChange Apply(string name, string location, string endpoint, string status, Guid actor, string actorName, DateTime now)
    {
        if (EndedAt.HasValue) throw new ArgumentException("Implantação encerrada; crie um novo período.");
        if (string.IsNullOrWhiteSpace(name) || name.Length > 120) throw new ArgumentException("Nome inválido.");
        if (location == null || location.Length > 200) throw new ArgumentException("Localização inválida.");
        if (endpoint == null || endpoint.Length > 300 || !InventoryResourceData.IsReferenceUrl(endpoint)) throw new ArgumentException("URL inválida.");
        if (!IsStatus(status)) throw new ArgumentException("Situação inválida.");
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Operador inválido.");
        Name = name; Location = location; Endpoint = endpoint; Status = status; ActorUserId = actor; Revision++;
        if (Revision > 1) UpdatedAt = now;
        if (status == "closed") EndedAt = now;
        return ServiceDeploymentChange.Create(this, actor, actorName, now);
    }
}
