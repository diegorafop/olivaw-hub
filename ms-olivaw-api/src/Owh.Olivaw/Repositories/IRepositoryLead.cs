using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;

namespace Owh.Olivaw.Repositories;

public interface IRepositoryLead : IRepository<Lead>
{
    Task<LeadReadPage> ReadAsync(LeadReadFilter filter, CancellationToken cancellationToken);
    Task<LeadReadSummary> ReadSummaryAsync(DateTime since, CancellationToken cancellationToken);
}
public sealed class LeadReadFilter
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
    public string? Entry { get; set; }
    public DateTime? From { get; set; }
    public DateTime? Until { get; set; }
}
public sealed class LeadReadItem
{
    public Guid ExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Business { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Entry { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
}
public sealed class LeadReadPage
{
    public long Total { get; set; }
    public List<LeadReadItem> Items { get; set; } = new();
}
public sealed class LeadReadSummary
{
    public long Total { get; set; }
    public long Recent { get; set; }
}
