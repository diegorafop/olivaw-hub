using System;
using System.Collections.Generic;
using Owh.OlivawCore.Domain;

namespace Owh.Olivaw.Domain;

public sealed class Customer : Entity
{
    public string Business { get; private set; } = string.Empty;
    public string LegalName { get; private set; } = string.Empty;
    public string? Document { get; private set; }
    public string Owner { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public string Status { get; private set; } = "onboarding";
    public long Revision { get; private set; }
    public int? SourceLeadId { get; private set; }
    public Lead? SourceLead { get; private set; }
    public ICollection<CustomerContact> Contacts { get; private set; } = new List<CustomerContact>();
    public ICollection<CustomerChange> Changes { get; private set; } = new List<CustomerChange>();

    private Customer() { }

    public static Customer Create(string business, string legalName, string? document, string owner, string notes, Guid actor, string actorName, DateTime now, Lead? source)
    {
        var customer = new Customer { ExternalId = Guid.NewGuid(), CreatedAt = now, SourceLead = source, SourceLeadId = source?.Id };
        customer.Apply(business, legalName, document, owner, notes, "onboarding", actor, actorName, now);
        customer.UpdatedAt = null;
        return customer;
    }

    public static bool CanTransition(string current, string next)
    {
        if (current == next) return true;
        return current switch
        {
            "onboarding" => next is "active" or "paused" or "closed",
            "active" => next is "paused" or "closed",
            "paused" => next is "onboarding" or "active" or "closed",
            "closed" => next == "onboarding",
            _ => false
        };
    }

    public void Apply(string business, string legalName, string? document, string owner, string notes, string status, Guid actor, string actorName, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(business) || business.Length > 120) throw new ArgumentException("Nome comercial inválido.", nameof(business));
        if (!CanTransition(Status, status)) throw new ArgumentException("Transição de situação inválida.", nameof(status));
        var previous = Status;
        Business = business;
        LegalName = legalName;
        Document = string.IsNullOrEmpty(document) ? null : document;
        Owner = owner;
        Notes = notes;
        Status = status;
        ActorUserId = actor;
        UpdatedAt = now;
        Revision++;
        Changes.Add(CustomerChange.Create(actor, actorName, Revision == 1 ? "created" : "updated", previous, status, now));
    }
}
