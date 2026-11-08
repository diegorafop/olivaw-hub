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
                SELECT externalid AS "ExternalId", createdat AS "CreatedAt", updatedat AS "UpdatedAt", followupstatus AS "FollowUpStatus",
                    COALESCE(snapshot::jsonb ->> 'business', '') AS "Business",
                    COALESCE(snapshot::jsonb -> 'project' ->> 'title', '') AS "Title",
                    COALESCE(snapshot::jsonb ->> 'entry', '') AS "Entry",
                    COALESCE(snapshot::jsonb ->> 'email', '') AS "Email",
                    COALESCE(NULLIF(snapshot::jsonb ->> 'whatsapp', ''), snapshot::jsonb -> 'qualification' ->> 'whatsapp', '') AS "Whatsapp",
                    COALESCE(snapshot::jsonb -> 'qualification' ->> 'phone', '') AS "Phone",
                    COALESCE(snapshot::jsonb -> 'qualification' ->> 'websiteUrl', '') AS "WebsiteUrl",
                    COALESCE(snapshot::jsonb -> 'qualification' ->> 'segment', '') AS "Segment",
                    COALESCE(snapshot::jsonb -> 'engagement' ->> 'level', 'unassessed') AS "Temperature",
                    COALESCE(snapshot::jsonb -> 'engagement' ->> 'reason', '') AS "TemperatureReason",
                    COALESCE(snapshot::jsonb -> 'engagement' ->> 'updatedAt', '') AS "TemperatureUpdatedAt"
                FROM lead WHERE deletedat IS NULL
                """;
        }
        else if (_database.IsSqlServer())
        {
            sql = """
                SELECT externalid AS [ExternalId], createdat AS [CreatedAt], updatedat AS [UpdatedAt], followupstatus AS [FollowUpStatus],
                    COALESCE(JSON_VALUE(snapshot, '$.business'), '') AS [Business],
                    COALESCE(JSON_VALUE(snapshot, '$.project.title'), '') AS [Title],
                    COALESCE(JSON_VALUE(snapshot, '$.entry'), '') AS [Entry],
                    COALESCE(JSON_VALUE(snapshot, '$.email'), '') AS [Email],
                    COALESCE(NULLIF(JSON_VALUE(snapshot, '$.whatsapp'), ''), JSON_VALUE(snapshot, '$.qualification.whatsapp'), '') AS [Whatsapp],
                    COALESCE(JSON_VALUE(snapshot, '$.qualification.phone'), '') AS [Phone],
                    COALESCE(JSON_VALUE(snapshot, '$.qualification.websiteUrl'), '') AS [WebsiteUrl],
                    COALESCE(JSON_VALUE(snapshot, '$.qualification.segment'), '') AS [Segment],
                    COALESCE(JSON_VALUE(snapshot, '$.engagement.level'), 'unassessed') AS [Temperature],
                    COALESCE(JSON_VALUE(snapshot, '$.engagement.reason'), '') AS [TemperatureReason],
                    COALESCE(JSON_VALUE(snapshot, '$.engagement.updatedAt'), '') AS [TemperatureUpdatedAt]
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
            query = query.Where(item => (item.Business + " " + item.Title + " " + item.Email + " " + item.Whatsapp + " " + item.Phone + " " + item.WebsiteUrl + " " + item.Segment).ToLower().Contains(term));
        }
        if (!string.IsNullOrEmpty(filter.FollowUpStatus)) query = query.Where(item => item.FollowUpStatus == filter.FollowUpStatus);
        if (!string.IsNullOrEmpty(filter.Entry)) query = query.Where(item => item.Entry == filter.Entry);
        if (filter.From.HasValue) query = query.Where(item => item.CreatedAt >= filter.From.Value);
        if (filter.Until.HasValue) query = query.Where(item => item.CreatedAt < filter.Until.Value);
        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.ExternalId)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
            .Select(item => new LeadReadItem
            {
                ExternalId = item.ExternalId, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt,
                Business = item.Business, Title = item.Title, Entry = item.Entry, Email = item.Email, Phone = item.Phone, WebsiteUrl = item.WebsiteUrl, Segment = item.Segment, Whatsapp = item.Whatsapp, FollowUpStatus = item.FollowUpStatus, Temperature = item.Temperature, TemperatureReason = item.TemperatureReason, TemperatureUpdatedAt = item.TemperatureUpdatedAt
            }).ToListAsync(cancellationToken);
        return new LeadReadPage { Total = total, Items = items };
    }

    public async Task<LeadReadSummary> ReadSummaryAsync(DateTime since, CancellationToken cancellationToken)
    {
        var leads = _context.Set<Lead>().AsNoTracking().Where(item => item.DeletedAt == null);
        var total = await leads.LongCountAsync(cancellationToken);
        var recent = await leads.LongCountAsync(item => item.CreatedAt >= since, cancellationToken);
        var byStatus = await leads.GroupBy(item => item.FollowUpStatus).Select(group => new { Status = group.Key, Total = group.LongCount() })
            .ToDictionaryAsync(item => item.Status, item => item.Total, cancellationToken);
        return new LeadReadSummary { Total = total, Recent = recent, ByStatus = byStatus };
    }
}

public sealed class LeadReadRow
{
    [Column("Temperature")] public string Temperature { get; set; } = "unassessed";
    [Column("TemperatureReason")] public string TemperatureReason { get; set; } = string.Empty;
    [Column("TemperatureUpdatedAt")] public string TemperatureUpdatedAt { get; set; } = string.Empty;
    [Column("ExternalId")] public Guid ExternalId { get; set; }
    [Column("CreatedAt")] public DateTime CreatedAt { get; set; }
    [Column("UpdatedAt")] public DateTime? UpdatedAt { get; set; }
    [Column("Business")] public string Business { get; set; } = string.Empty;
    [Column("Title")] public string Title { get; set; } = string.Empty;
    [Column("Entry")] public string Entry { get; set; } = string.Empty;
    [Column("Email")] public string Email { get; set; } = string.Empty;
    [Column("FollowUpStatus")] public string FollowUpStatus { get; set; } = "new";
    [Column("Phone")] public string Phone { get; set; } = string.Empty;
    [Column("WebsiteUrl")] public string WebsiteUrl { get; set; } = string.Empty;
    [Column("Segment")] public string Segment { get; set; } = string.Empty;
    [Column("Whatsapp")] public string Whatsapp { get; set; } = string.Empty;
}
