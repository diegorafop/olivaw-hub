using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;

namespace Owh.Olivaw.Dados.Repositories;

// Consulta especializada: projeção JSON e paginação no banco, sem carregar chaves ou snapshots completos na lista.
public sealed class RepositoryLead(WriteDbContext context) : Repository<Lead>(context), IRepositoryLead
{
    private readonly DatabaseFacade _database = context.Database;

    public IQueryable<LeadReadRow> ReadRows()
    {
        string sql;
        if (_database.IsNpgsql())
        {
            sql = """
                SELECT externalid AS "ExternalId", createdat AS "CreatedAt", updatedat AS "UpdatedAt",
                    COALESCE(snapshot::jsonb ->> 'business', '') AS "Business",
                    COALESCE(snapshot::jsonb -> 'project' ->> 'title', '') AS "Title",
                    COALESCE(snapshot::jsonb ->> 'entry', '') AS "Entry",
                    COALESCE(snapshot::jsonb ->> 'email', '') AS "Email",
                    COALESCE(snapshot::jsonb ->> 'whatsapp', '') AS "Whatsapp"
                FROM lead WHERE deletedat IS NULL
                """;
        }
        else if (_database.IsSqlServer())
        {
            sql = """
                SELECT externalid AS [ExternalId], createdat AS [CreatedAt], updatedat AS [UpdatedAt],
                    COALESCE(JSON_VALUE(snapshot, '$.business'), '') AS [Business],
                    COALESCE(JSON_VALUE(snapshot, '$.project.title'), '') AS [Title],
                    COALESCE(JSON_VALUE(snapshot, '$.entry'), '') AS [Entry],
                    COALESCE(JSON_VALUE(snapshot, '$.email'), '') AS [Email],
                    COALESCE(JSON_VALUE(snapshot, '$.whatsapp'), '') AS [Whatsapp]
                FROM lead WHERE deletedat IS NULL
                """;
        }
        else
        {
            throw new NotSupportedException("Provedor não suportado para leitura de leads.");
        }
        return _database.SqlQueryRaw<LeadReadRow>(sql);
    }

    public async Task<LeadReadPage> ReadAsync(LeadReadFilter filter, CancellationToken cancellationToken)
    {
        var query = ReadRows();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.ToLowerInvariant();
            query = query.Where(item => (item.Business + " " + item.Title + " " + item.Email + " " + item.Whatsapp).ToLower().Contains(term));
        }
        if (!string.IsNullOrEmpty(filter.Entry)) query = query.Where(item => item.Entry == filter.Entry);
        if (filter.From.HasValue) query = query.Where(item => item.CreatedAt >= filter.From.Value);
        if (filter.Until.HasValue) query = query.Where(item => item.CreatedAt < filter.Until.Value);
        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.ExternalId)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
            .Select(item => new LeadReadItem
            {
                ExternalId = item.ExternalId, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt,
                Business = item.Business, Title = item.Title, Entry = item.Entry, Email = item.Email, Whatsapp = item.Whatsapp
            }).ToListAsync(cancellationToken);
        return new LeadReadPage { Total = total, Items = items };
    }

    public async Task<LeadReadSummary> ReadSummaryAsync(DateTime since, CancellationToken cancellationToken)
    {
        var leads = _context.Set<Lead>().AsNoTracking().Where(item => item.DeletedAt == null);
        var total = await leads.LongCountAsync(cancellationToken);
        var recent = await leads.LongCountAsync(item => item.CreatedAt >= since, cancellationToken);
        return new LeadReadSummary { Total = total, Recent = recent };
    }
}

public sealed class LeadReadRow
{
    [Column("ExternalId")] public Guid ExternalId { get; set; }
    [Column("CreatedAt")] public DateTime CreatedAt { get; set; }
    [Column("UpdatedAt")] public DateTime? UpdatedAt { get; set; }
    [Column("Business")] public string Business { get; set; } = string.Empty;
    [Column("Title")] public string Title { get; set; } = string.Empty;
    [Column("Entry")] public string Entry { get; set; } = string.Empty;
    [Column("Email")] public string Email { get; set; } = string.Empty;
    [Column("Whatsapp")] public string Whatsapp { get; set; } = string.Empty;
}
