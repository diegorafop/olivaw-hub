using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class SecurityRemediationExtensions
{
    internal static void SetupSecurityRemediation(this ModelBuilder builder)
    {
        var entity = builder.Entity<SecurityRemediation>();
        entity.ToTable("securityremediation"); entity.HasKey(item => item.Id);
        entity.HasIndex(item => item.ExternalId).IsUnique();
        entity.HasIndex(item => new { item.SecurityReportId, item.FindingIndex }).IsUnique();
        entity.HasIndex(item => new { item.ExecutorUserId, item.HostId, item.Status });
        entity.HasOne(item => item.SecurityReport).WithMany().HasForeignKey(item => item.SecurityReportId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(item => item.ActiveHostKey).HasMaxLength(128);
        entity.HasIndex(item => item.ActiveHostKey).IsUnique().HasFilter("activehostkey IS NOT NULL");
        entity.Property(item => item.Status).HasMaxLength(24).IsRequired();
        entity.Property(item => item.HostId).HasMaxLength(128).IsRequired();
        entity.Property(item => item.Message).HasMaxLength(1000).IsRequired();
        entity.Property(item => item.PlanJson).IsRequired(); entity.Property(item => item.ResultJson).IsRequired();
        entity.Property(item => item.Revision).IsConcurrencyToken();
    }
}
