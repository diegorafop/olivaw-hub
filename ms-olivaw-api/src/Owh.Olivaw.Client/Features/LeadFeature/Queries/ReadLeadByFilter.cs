using System;
using System.Collections.Generic;

namespace Owh.Olivaw.Client.Features.LeadFeature.Queries;

public sealed class ReadLeadByFilterQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Entry { get; set; }
    public string? FollowUpStatus { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? Until { get; set; }
}
public sealed class ReadLeadByFilterResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
    public DateTimeOffset AsOf { get; set; }
    public List<LeadListItem> Items { get; set; } = [];
}
public sealed class LeadListItem
{
    public Guid ExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Business { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Entry { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
    public string FollowUpStatus { get; set; } = "new";
}
