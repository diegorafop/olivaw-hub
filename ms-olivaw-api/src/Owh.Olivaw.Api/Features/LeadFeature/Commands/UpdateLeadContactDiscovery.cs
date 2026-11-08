using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Npgsql;
using Owh.Olivaw.Client.Features.LeadFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.LeadFeature.Commands;

public sealed class UpdateLeadContactDiscoveryMediatorCommand : IRequest<UpdateLeadContactDiscoveryResponse>
{
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public UpdateLeadContactDiscoveryCommand Payload { get; set; } = new();
}
public sealed class UpdateLeadContactDiscoveryHandler(IRepository<Lead> repository, IRepository<LeadChange> changes)
    : IRequestHandler<UpdateLeadContactDiscoveryMediatorCommand, UpdateLeadContactDiscoveryResponse>
{
    public async Task<UpdateLeadContactDiscoveryResponse> Handle(UpdateLeadContactDiscoveryMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var lead = await repository.GetFirstAsync(item => item.ExternalId == request.ExternalId && item.DeletedAt == null, cancellationToken);
        var payload = request.Payload;
        if (lead.Revision != payload.Revision || lead.FollowUpRevision != payload.FollowUpRevision) return new() { Conflict = true };
        var snapshot = JsonNode.Parse(lead.Snapshot)!.AsObject();
        var qualification = snapshot["qualification"]?.AsObject() ?? new JsonObject { ["schemaVersion"] = 1 };
        var incoming = JsonSerializer.SerializeToNode(payload.Contacts, new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject();
        var emails = qualification["publicEmails"]?.AsArray() ?? new JsonArray();
        foreach (var email in payload.Contacts.PublicEmails)
        {
            if (emails.Count >= 5) break;
            if (!emails.Any(item => string.Equals(item?["email"]?.GetValue<string>(), email.Email, StringComparison.OrdinalIgnoreCase)))
                emails.Add(JsonSerializer.SerializeToNode(email, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }
        qualification["publicEmails"] = emails;
        foreach (var field in new[] { "contactPageUrl", "contactFormUrl" })
        {
            if (string.IsNullOrEmpty(qualification[field]?.GetValue<string>())) qualification[field] = incoming[field]?.DeepClone();
        }
        qualification["contactFormStatus"] = incoming["contactFormStatus"]?.DeepClone();
        if (!string.IsNullOrEmpty(qualification["contactFormUrl"]?.GetValue<string>())) qualification["contactFormStatus"] = "found";
        qualification["contactCheckedAt"] = incoming["contactCheckedAt"]?.DeepClone();
        snapshot["qualification"] = qualification;
        var primary = snapshot["email"]?.GetValue<string>() ?? string.Empty;
        var note = string.Empty;
        if (string.IsNullOrEmpty(primary) && emails.Count == 1)
        {
            primary = emails[0]!["email"]!.GetValue<string>();
            snapshot["email"] = primary;
            note = "E-mail principal preenchido a partir de fonte pública registrada.";
        }
        var discovery = snapshot["contactDiscovery"]?.AsObject() ?? new JsonObject();
        var status = "inconclusive";
        if (!string.IsNullOrEmpty(primary) || emails.Count > 0) status = "found";
        else if (payload.ResearchCompleted) status = "not-found";
        if (emails.Count > 1 && string.IsNullOrEmpty(primary)) status = "conflict";
        var noEmailNoted = discovery["noEmailNoted"]?.GetValue<bool>() == true;
        if (status == "not-found" && !noEmailNoted)
        {
            note = "sem email";
            discovery["noEmailNoted"] = true;
        }
        discovery["status"] = status;
        discovery["checkedAt"] = incoming["contactCheckedAt"]?.DeepClone();
        snapshot["contactDiscovery"] = discovery;
        snapshot["revision"] = lead.Revision + 1;
        var serialized = snapshot.ToJsonString();
        var size = new InlineValidator<string>();
        size.RuleFor(item => item).MaximumLength(32000);
        await size.ValidateAndThrowAsync(serialized, cancellationToken);
        var now = DateTime.UtcNow;
        lead.Apply(lead.Revision + 1, serialized, now);
        lead.ActorUserId = request.ActorUserId;
        if (!string.IsNullOrEmpty(note))
        {
            var stage = lead.FollowUpStatus;
            if (note == "sem email" && stage == "new") stage = "conversation";
            var change = lead.UpdateFollowUp(stage, note, request.ActorUserId, request.ActorName, now);
            await changes.AddAsync(change, cancellationToken);
        }
        await repository.UpdateAsync(lead, cancellationToken);
        try { await repository.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return new() { Conflict = true }; }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "23505" }
            or SqlException { Number: 2601 or 2627 }) { return new() { Conflict = true }; }
        return new() { Revision = lead.Revision, FollowUpRevision = lead.FollowUpRevision, Status = status };
    }
}
public sealed class UpdateLeadContactDiscoveryValidator : AbstractValidator<UpdateLeadContactDiscoveryCommand>
{
    public UpdateLeadContactDiscoveryValidator()
    {
        RuleFor(item => item.Revision).InclusiveBetween(1, 1_000_000);
        RuleFor(item => item.FollowUpRevision).InclusiveBetween(0, long.MaxValue - 1);
        RuleFor(item => item.Contacts).NotNull();
        RuleFor(item => new UpdateLeadQualificationCommand { Revision = item.Revision, Qualification = item.Contacts })
            .SetValidator(new UpdateLeadQualificationValidator());
        RuleFor(item => item.Contacts.ContactFormStatus).Must(value => value is "found" or "not-detected").When(item => item.ResearchCompleted && item.Contacts != null);
        RuleFor(item => item.Contacts.ContactCheckedAt).NotNull().When(item => item.Contacts != null);
    }
}
public sealed class UpdateLeadContactDiscoveryMediatorValidator : AbstractValidator<UpdateLeadContactDiscoveryMediatorCommand>
{
    public UpdateLeadContactDiscoveryMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateLeadContactDiscoveryValidator()).OverridePropertyName(string.Empty);
    }
}
