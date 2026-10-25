using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;

namespace Owh.Olivaw.Dados.Extensions.Tables;

internal static class SecurityReportExtensions
{
    internal static void SetupSecurityReport(this ModelBuilder builder)
    {
        var entity = builder.Entity<SecurityReport>();
        entity.ToTable("securityreport");
        entity.HasKey(item => item.Id);
        entity.HasIndex(item => item.ExternalId).IsUnique();
        entity.HasIndex(item => new { item.AgentId, item.RunId }).IsUnique();
        entity.HasIndex(item => new { item.HostId, item.CompletedAt });
        entity.Property(item => item.AgentId).HasMaxLength(64).IsRequired();
        entity.Property(item => item.HostId).HasMaxLength(128).IsRequired();
        entity.Property(item => item.SchemaVersion).HasMaxLength(20).IsRequired();
        entity.Property(item => item.ExecutionStatus).HasMaxLength(20).IsRequired();
        entity.Property(item => item.RawData).IsRequired();
    }
}
