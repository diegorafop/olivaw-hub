using System.Text.Json.Nodes;
using MediatR;
using Owh.Olivaw.Client.Features.LeadFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Api.Features.LeadFeature.Queries;

public sealed class ReadLeadCommunicationSummaryMediatorQuery : IRequest<ReadLeadCommunicationSummaryResponse> { }
public sealed class ReadLeadCommunicationSummaryHandler(IRepository<Lead> repository)
    : IRequestHandler<ReadLeadCommunicationSummaryMediatorQuery, ReadLeadCommunicationSummaryResponse>
{
    public async Task<ReadLeadCommunicationSummaryResponse> Handle(ReadLeadCommunicationSummaryMediatorQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        // Amostra explicitamente limitada; não materializar o cadastro inteiro nem alegar total global.
        var leads = (await repository.TakeOrderedAsync(item => item.DeletedAt == null, 501, item => item.CreatedAt, cancellationToken)).ToList();
        var response = new ReadLeadCommunicationSummaryResponse { Inspected = Math.Min(leads.Count, 500), Truncated = leads.Count > 500, AsOf = DateTime.UtcNow };
        foreach (var key in new[] { "collected", "enriched", "noEmail", "inconclusive", "ready", "prepared", "sent", "received", "pending", "drafts", "suppressed", "handoff", "human", "cancelled", "unreadable" }) response.Counts[key] = 0;
        foreach (var lead in leads.Take(500))
        {
            JsonNode? snapshot;
            try { snapshot = JsonNode.Parse(lead.Snapshot); }
            catch (System.Text.Json.JsonException) { response.Counts["unreadable"]++; continue; }
            if (snapshot is not JsonObject) { response.Counts["unreadable"]++; continue; }
            if (Text(snapshot, "entry") != "prospecting") continue;
            response.Counts["collected"]++;
            var discovery = snapshot["contactDiscovery"];
            var status = Text(discovery, "status");
            if (status == "found") response.Counts["enriched"]++;
            if (status == "not-found") response.Counts["noEmail"]++;
            if (status is "inconclusive" or "conflict") response.Counts["inconclusive"]++;
            var entries = snapshot["communications"] as JsonArray ?? new JsonArray();
            var kinds = entries.Select(item => Text(item, "kind")).ToHashSet();
            if (kinds.Contains("suppressed")) response.Counts["suppressed"]++;
            if (kinds.Contains("handoff")) response.Counts["handoff"]++;
            if (kinds.Contains("human-takeover")) response.Counts["human"]++;
            var hasEmail = !string.IsNullOrEmpty(Text(snapshot, "email"));
            var qualification = snapshot["qualification"] as JsonObject;
            hasEmail |= qualification?["publicEmails"] is JsonArray { Count: > 0 };
            var blocked = kinds.Overlaps(new[] { "suppressed", "handoff", "human-takeover" });
            var tags = qualification?["tags"] as JsonArray ?? new JsonArray();
            blocked |= tags.Any(item => Scalar(item).ToLowerInvariant() is "opt-out" or "do-not-contact" or "não contatar" or "nao contatar");
            if (hasEmail && !blocked && lead.FollowUpStatus is "new" or "conversation") response.Counts["ready"]++;
            foreach (var entry in entries)
            {
                var kind = Text(entry, "kind");
                var intent = Text(entry, "intentId");
                if (kind == "sent") response.Counts["sent"]++;
                if (kind == "received") response.Counts["received"]++;
                if (kind == "reply-draft") response.Counts["drafts"]++;
                if (kind == "cancelled") response.Counts["cancelled"]++;
                var terminal = entries.Any(item => Text(item, "intentId") == intent && Text(item, "kind") is "sent" or "cancelled");
                if (kind == "sending" && !terminal) response.Counts["pending"]++;
                if (kind == "prepared" && !terminal && !entries.Any(item => Text(item, "intentId") == intent && Text(item, "kind") == "sending")) response.Counts["prepared"]++;
            }
        }
        return response;
    }
    private static string Text(JsonNode? node, string property)
    {
        if (node is not JsonObject value) return string.Empty;
        return Scalar(value[property]);
    }
    private static string Scalar(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text)) return text;
        return string.Empty;
    }
}
