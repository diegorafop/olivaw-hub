using System;
using System.Collections.Generic;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class SupportRequest : Entity
{
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public int? ManagedServiceId { get; private set; }
    public ManagedService? ManagedService { get; private set; }
    public string Protocol { get; private set; } = string.Empty;
    public string PublicSummary { get; private set; } = string.Empty;
    public Guid? PortalSubmissionKey { get; private set; }
    public string SubmissionDigest { get; private set; } = string.Empty;
    public string RequestType { get; private set; } = string.Empty;
    public decimal? DesiredStorageGB { get; private set; }
    public decimal? DesiredMemoryMiB { get; private set; }
    public string DesiredPlan { get; private set; } = string.Empty;
    public long Revision { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Requester { get; private set; } = string.Empty;
    public string Impact { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public string NextAction { get; private set; } = string.Empty;
    public string InternalNote { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Status { get; private set; } = "received";
    public ICollection<SupportRequestChange> Changes { get; private set; } = new List<SupportRequestChange>();
    private SupportRequest() { }
    public static SupportRequest Create(Customer anchor, SupportRequestData data, Guid actor, string actorName, DateTime now, ManagedService? service)
    {
        if (anchor == null) throw new ArgumentNullException(nameof(anchor));
        if (data.Status != "received") throw new ArgumentException("Estado inicial inválido.");
        var entity = new SupportRequest { ExternalId = Guid.NewGuid(), Customer = anchor, CustomerId = anchor.Id, ManagedService = service, ManagedServiceId = service?.Id, Protocol = "OW-" + now.ToString("yyyy") + "-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant(), CreatedAt = now };
        entity.Changes.Add(entity.Apply(data, actor, actorName, now));
        return entity;
    }
    public void RecordPortalSubmission(Guid key, string digest, string requestType, decimal? storage, decimal? memory, string plan)
    {
        if (PortalSubmissionKey.HasValue || Revision != 1) throw new ArgumentException("Origem da solicitação já registrada.");
        if (key == Guid.Empty || digest.Length != 64) throw new ArgumentException("Origem inválida.");
        if (requestType is not ("help" or "question" or "improvement" or "storage" or "capacity" or "plan")) throw new ArgumentException("Pedido inválido.");
        if (storage <= 0 || memory <= 0 || storage > 1000000000 || memory > 1000000000) throw new ArgumentException("Capacidade inválida.");
        if (plan == null || plan.Length > 120) throw new ArgumentException("Plano inválido.");
        PortalSubmissionKey = key; SubmissionDigest = digest; RequestType = requestType; DesiredStorageGB = storage; DesiredMemoryMiB = memory; DesiredPlan = plan;
    }
    public SupportRequestChange Apply(SupportRequestData data, Guid actor, string actorName, DateTime now)
    {
        data.Validate();
        if (actor == Guid.Empty || string.IsNullOrWhiteSpace(actorName) || actorName.Length > 120) throw new ArgumentException("Autor obrigatório.");
        Title = data.Title;
        Requester = data.Requester;
        Impact = data.Impact;
        Owner = data.Owner;
        NextAction = data.NextAction;
        InternalNote = data.InternalNote;
        PublicSummary = data.PublicSummary;
        Category = data.Category; Status = data.Status;
        ActorUserId = actor; Revision++;
        if (Revision > 1) UpdatedAt = now;
        return SupportRequestChange.Create(this, actorName, now);
    }
}
