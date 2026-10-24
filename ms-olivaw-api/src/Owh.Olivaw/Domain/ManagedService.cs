using System;
using System.Collections.Generic;
using System.Linq;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class ManagedService : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string TechnicalId { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public string Scope { get; private set; } = string.Empty;
    public string Status { get; private set; } = "planned";
    public long Revision { get; private set; }
    public ICollection<ManagedServiceCustomer> Customers { get; private set; } = new List<ManagedServiceCustomer>();
    public ICollection<ManagedServiceChange> Changes { get; private set; } = new List<ManagedServiceChange>();
    private ManagedService() { }

    public static bool IsType(string type) => type is "frontend" or "api" or "worker" or "scheduled" or "other";
    public static bool IsStatus(string status) => status is "planned" or "onboarding" or "active" or "closed";
    public static bool IsScope(string scope) => scope is "internal" or "customer";

    public static ManagedService Create(string technicalId, string name, string type, string description, string owner, string scope, string status, Guid actor, string actorName, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(technicalId)) throw new ArgumentException("Identificador obrigatório.", nameof(technicalId));
        if (technicalId.Length > 80) throw new ArgumentException("Identificador muito longo.", nameof(technicalId));
        if (!System.Text.RegularExpressions.Regex.IsMatch(technicalId, "^[a-z0-9]+(?:-[a-z0-9]+)*$")) throw new ArgumentException("Identificador inválido.", nameof(technicalId));
        if (actor == Guid.Empty) throw new ArgumentException("Operador obrigatório.", nameof(actor));
        var service = new ManagedService { ExternalId = Guid.NewGuid(), TechnicalId = technicalId, CreatedAt = now, ActorUserId = actor };
        service.Changes.Add(service.Apply(name, type, description, owner, scope, status, actor, actorName, now));
        return service;
    }

    public ManagedServiceChange Apply(string name, string type, string description, string owner, string scope, string status, Guid actor, string actorName, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome obrigatório.", nameof(name));
        if (name.Length > 120) throw new ArgumentException("Nome muito longo.", nameof(name));
        if (!IsType(type)) throw new ArgumentException("Tipo inválido.", nameof(type));
        if (!IsStatus(status)) throw new ArgumentException("Situação inválida.", nameof(status));
        if (!IsScope(scope)) throw new ArgumentException("Escopo inválido.", nameof(scope));
        if (description == null) throw new ArgumentNullException(nameof(description));
        if (description.Length > 1000) throw new ArgumentException("Descrição muito longa.", nameof(description));
        if (owner == null) throw new ArgumentNullException(nameof(owner));
        if (owner.Length > 120) throw new ArgumentException("Responsável muito longo.", nameof(owner));
        if (actor == Guid.Empty) throw new ArgumentException("Operador obrigatório.", nameof(actor));
        if (string.IsNullOrWhiteSpace(actorName)) throw new ArgumentException("Operador obrigatório.", nameof(actorName));
        if (actorName.Length > 120) throw new ArgumentException("Operador inválido.", nameof(actorName));
        var previous = Status;
        Name = name; Type = type; Description = description; Owner = owner; Scope = scope; Status = status;
        ActorUserId = actor; Revision++;
        if (Revision > 1) UpdatedAt = now;
        return ManagedServiceChange.Create(this, previous, actor, actorName, now);
    }
}
