using System;
using System.Collections.Generic;

namespace Owh.Olivaw.Client.Features.ManagedServiceFeature.Queries;

public sealed class ReadManagedServiceByFilterQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
    public string? Type { get; set; } = string.Empty;
    public Guid? CustomerExternalId { get; set; }
}
public sealed class ReadManagedServiceByFilterResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
    public List<ManagedServiceListItem> Items { get; set; } = new();
}
public sealed class ManagedServiceListItem
{
    public Guid ExternalId { get; set; }
    public string TechnicalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
}
