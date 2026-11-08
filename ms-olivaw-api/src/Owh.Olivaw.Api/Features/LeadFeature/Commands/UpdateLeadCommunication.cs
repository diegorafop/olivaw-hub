using System.Security.Cryptography;
using System.Text;
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

public sealed class UpdateLeadCommunicationMediatorCommand : IRequest<UpdateLeadCommunicationResponse>
{
    public Guid ExternalId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public UpdateLeadCommunicationCommand Payload { get; set; } = new();
}
public sealed class UpdateLeadCommunicationHandler(IRepository<Lead> repository, IRepository<LeadChange> changes)
    : IRequestHandler<UpdateLeadCommunicationMediatorCommand, UpdateLeadCommunicationResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public async Task<UpdateLeadCommunicationResponse> Handle(UpdateLeadCommunicationMediatorCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var lead = await repository.GetFirstAsync(item => item.ExternalId == request.ExternalId && item.DeletedAt == null, cancellationToken);
        var payload = request.Payload;
        var record = payload.Event;
        var snapshot = JsonNode.Parse(lead.Snapshot)!.AsObject();
        var entries = snapshot["communications"]?.AsArray() ?? new JsonArray();
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(record, JsonOptions))));
        var previous = entries.FirstOrDefault(item => item?["eventId"]?.GetValue<string>() == record.EventId);
        if (previous != null)
        {
            var duplicateValidator = new InlineValidator<string>();
            duplicateValidator.RuleFor(item => item).Equal(previous["fingerprint"]!.GetValue<string>()).WithMessage("Evento existente possui outro conteúdo.");
            await duplicateValidator.ValidateAndThrowAsync(fingerprint, cancellationToken);
            return new() { Repeated = true, Revision = lead.Revision, FollowUpRevision = lead.FollowUpRevision };
        }
        if (lead.Revision != payload.Revision || lead.FollowUpRevision != payload.FollowUpRevision) return new() { Conflict = true };
        var validator = new InlineValidator<LeadCommunicationEvent>();
        validator.RuleFor(item => entries.Count).LessThan(40).OverridePropertyName("Event").WithMessage("Limite do piloto atingido; revisar histórico sem apagá-lo.");
        var lastResume = -1;
        for (var index = 0; index < entries.Count; index++)
            if (entries[index]?["kind"]?.GetValue<string>() == "resumed") lastResume = index;
        var activeEntries = entries.Skip(lastResume + 1).ToArray();
        if (record.Kind is "prepared" or "sending" or "resumed")
        {
            validator.RuleFor(item => lead.FollowUpStatus).Must(status => status is "new" or "conversation").OverridePropertyName("Event");
            var initiative = snapshot["acquisition"]?["initiative"]?.GetValue<string>();
            validator.RuleFor(item => initiative).Equal("outbound").OverridePropertyName("Event");
            var allowed = snapshot["email"]?.GetValue<string>()?.Equals(record.Email, StringComparison.OrdinalIgnoreCase) == true;
            var qualification = snapshot["qualification"];
            allowed |= qualification?["publicEmails"]?.AsArray().Any(item => item?["email"]?.GetValue<string>()?.Equals(record.Email, StringComparison.OrdinalIgnoreCase) == true) == true;
            validator.RuleFor(item => allowed).Equal(true).OverridePropertyName("Event.Email");
            var tags = qualification?["tags"]?.AsArray().Select(item => item?.GetValue<string>()?.ToLowerInvariant()).ToArray() ?? [];
            validator.RuleFor(item => tags).Must(items => !items.Any(tag => tag is "opt-out" or "do-not-contact" or "não contatar" or "nao contatar")).OverridePropertyName("Event");
            validator.RuleFor(item => entries).Must(items => !items.Any(entry => entry?["kind"]?.GetValue<string>() == "suppressed")).OverridePropertyName("Event");
            if (record.Kind != "resumed")
                validator.RuleFor(item => activeEntries).Must(items => !items.Any(entry => entry?["kind"]?.GetValue<string>() is "human-takeover" or "handoff")).OverridePropertyName("Event");
        }
        if (record.Kind == "resumed")
        {
            var pending = entries.Any(entry => entry?["kind"]?.GetValue<string>() == "sending" && !entries.Any(sent => sent?["kind"]?.GetValue<string>() == "sent" && sent?["intentId"]?.GetValue<string>() == entry?["intentId"]?.GetValue<string>()));
            validator.RuleFor(item => pending).Equal(false).OverridePropertyName("Event").WithMessage("Reconcilie os envios pendentes antes de retomar.");
            validator.RuleFor(item => activeEntries).Must(items => items.Any(entry => entry?["kind"]?.GetValue<string>() is "human-takeover" or "handoff" && entry?["gmailMessageId"]?.GetValue<string>() == record.GmailMessageId)).OverridePropertyName("Event");
            validator.RuleFor(item => item.BodyPreview).NotEmpty();
        }
        bool IsCancelled(JsonNode? entry) => entries.Any(cancelled => cancelled?["kind"]?.GetValue<string>() == "cancelled" && cancelled?["intentId"]?.GetValue<string>() == entry?["intentId"]?.GetValue<string>());
        if (record.Kind == "prepared")
        {
            if (string.IsNullOrEmpty(record.ReplyMessageId))
            {
                validator.RuleFor(item => entries).Must(items => !items.Any(entry => entry?["kind"]?.GetValue<string>() == "prepared" && !IsCancelled(entry) && string.IsNullOrEmpty(entry?["replyMessageId"]?.GetValue<string>()))).OverridePropertyName("Event");
            }
            else
            {
                validator.RuleFor(item => entries).Must(items => items.Any(entry => entry?["kind"]?.GetValue<string>() == "received" && entry?["gmailMessageId"]?.GetValue<string>() == record.ReplyMessageId && entry?["threadId"]?.GetValue<string>() == record.ThreadId)).OverridePropertyName("Event");
                validator.RuleFor(item => entries).Must(items => !items.Any(entry => entry?["kind"]?.GetValue<string>() == "prepared" && !IsCancelled(entry) && entry?["replyMessageId"]?.GetValue<string>() == record.ReplyMessageId)).OverridePropertyName("Event");
            }
        }
        if (record.Kind == "cancelled")
        {
            validator.RuleFor(item => entries).Must(items => items.Any(entry => entry?["kind"]?.GetValue<string>() == "prepared" && entry?["intentId"]?.GetValue<string>() == record.IntentId)).OverridePropertyName("Event");
            validator.RuleFor(item => entries).Must(items => !items.Any(entry => entry?["kind"]?.GetValue<string>() is "sending" or "sent" && entry?["intentId"]?.GetValue<string>() == record.IntentId)).OverridePropertyName("Event");
        }
        if (record.Kind is "sending" or "sent")
        {
            validator.RuleFor(item => entries).Must(items => !items.Any(entry => entry?["kind"]?.GetValue<string>() == "cancelled" && entry?["intentId"]?.GetValue<string>() == record.IntentId)).OverridePropertyName("Event");
            var required = "prepared";
            if (record.Kind == "sent") required = "sending";
            validator.RuleFor(item => entries).Must(items => items.Any(entry => entry?["kind"]?.GetValue<string>() == required && entry?["intentId"]?.GetValue<string>() == record.IntentId && entry?["contentHash"]?.GetValue<string>() == record.ContentHash && entry?["email"]?.GetValue<string>() == record.Email && entry?["subject"]?.GetValue<string>() == record.Subject && entry?["replyMessageId"]?.GetValue<string>() == record.ReplyMessageId)).OverridePropertyName("Event");
        }
        if (record.Kind is "received" or "review-required" or "human-takeover" or "suppressed" or "handoff" or "reply-draft" or "resumed")
        {
            validator.RuleFor(item => entries).Must(items => items.Any(entry => entry?["kind"]?.GetValue<string>() == "sent" && entry?["threadId"]?.GetValue<string>() == record.ThreadId)).OverridePropertyName("Event");
        }
        await validator.ValidateAndThrowAsync(record, cancellationToken);
        // Corpos ficam nas notas do histórico, que não infla o snapshot de 32 KB.
        var entry = JsonSerializer.SerializeToNode(record, JsonOptions)!.AsObject();
        entry.Remove("bodyPreview");
        entry["fingerprint"] = fingerprint;
        entry["recordedAt"] = DateTime.UtcNow;
        entry["actor"] = request.ActorName;
        entries.Add(entry);
        snapshot["communications"] = entries;
        snapshot["revision"] = lead.Revision + 1;
        var serialized = snapshot.ToJsonString();
        var size = new InlineValidator<string>();
        size.RuleFor(item => item).MaximumLength(32000);
        await size.ValidateAndThrowAsync(serialized, cancellationToken);
        var now = DateTime.UtcNow;
        lead.Apply(lead.Revision + 1, serialized, now);
        var status = lead.FollowUpStatus;
        if (status == "new" && record.Kind is "sent" or "received") status = "conversation";
        var note = $"Gmail · {record.Kind} · {record.Email}\nAssunto: {record.Subject}\nMensagem: {record.GmailMessageId} · Thread: {record.ThreadId}\n{record.BodyPreview}";
        var change = lead.UpdateFollowUp(status, note, request.ActorUserId, request.ActorName, now);
        lead.ActorUserId = request.ActorUserId;
        await repository.UpdateAsync(lead, cancellationToken);
        await changes.AddAsync(change, cancellationToken);
        try { await repository.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return new() { Conflict = true }; }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "23505" }
            or SqlException { Number: 2601 or 2627 }) { return new() { Conflict = true }; }
        return new() { Revision = lead.Revision, FollowUpRevision = lead.FollowUpRevision };
    }
}
public sealed class UpdateLeadCommunicationValidator : AbstractValidator<UpdateLeadCommunicationCommand>
{
    public UpdateLeadCommunicationValidator()
    {
        RuleFor(item => item.Revision).InclusiveBetween(1, 1_000_000);
        RuleFor(item => item.FollowUpRevision).InclusiveBetween(0, long.MaxValue - 1);
        RuleFor(item => item.Event).NotNull();
        When(item => item.Event != null, () => RuleFor(item => item.Event).ChildRules(record =>
        {
            record.RuleFor(item => item.EventId).NotEmpty().Matches("^[a-f0-9]{64}$");
            record.RuleFor(item => item.IntentId).NotEmpty().Matches("^[a-f0-9]{64}$");
            record.RuleFor(item => item.ContentHash).NotEmpty().Matches("^[a-f0-9]{64}$");
            record.RuleFor(item => item.Kind).Must(value => value is "prepared" or "sending" or "sent" or "received" or "review-required" or "human-takeover" or "cancelled" or "suppressed" or "handoff" or "reply-draft" or "resumed");
            record.RuleFor(item => item.Email).NotEmpty().MaximumLength(254).EmailAddress().Must(value => value == null || !value.Any(char.IsControl));
            record.RuleFor(item => item.Subject).NotNull().MaximumLength(200).Must(value => value == null || !value.Any(char.IsControl));
            record.RuleFor(item => item.QuestionId).Must(value => value is "" or "need" or "current-system" or "deadline" or "contact-person");
            record.RuleFor(item => item.BodyPreview).NotNull().MaximumLength(1500);
            record.RuleFor(item => item.GmailMessageId).NotNull().MaximumLength(200).Matches("^[a-zA-Z0-9_-]*$");
            record.RuleFor(item => item.ThreadId).NotNull().MaximumLength(200).Matches("^[a-zA-Z0-9_-]*$");
            record.RuleFor(item => item.ReplyMessageId).NotNull().MaximumLength(200).Matches("^[a-zA-Z0-9_-]*$");
            record.RuleFor(item => item.GmailMessageId).NotEmpty().When(item => item.Kind is "sent" or "received" or "review-required" or "human-takeover" or "suppressed" or "handoff" or "reply-draft" or "resumed");
            record.RuleFor(item => item.ThreadId).NotEmpty().When(item => item.Kind is "sent" or "received" or "review-required" or "human-takeover" or "suppressed" or "handoff" or "reply-draft" or "resumed");
        }));
    }
}
public sealed class UpdateLeadCommunicationMediatorValidator : AbstractValidator<UpdateLeadCommunicationMediatorCommand>
{
    public UpdateLeadCommunicationMediatorValidator()
    {
        RuleFor(item => item.ExternalId).NotEmpty();
        RuleFor(item => item.ActorUserId).NotEmpty();
        RuleFor(item => item.ActorName).NotEmpty().MaximumLength(120);
        RuleFor(item => item.Payload).NotNull().SetValidator(new UpdateLeadCommunicationValidator()).OverridePropertyName(string.Empty);
    }
}
