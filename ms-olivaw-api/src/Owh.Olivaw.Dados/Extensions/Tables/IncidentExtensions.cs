using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class IncidentExtensions
{
    internal static void SetupIncident(this ModelBuilder builder)
    {
        var entity = builder.Entity<Incident>(); entity.ToTable("incident"); entity.HasKey(item => item.Id);
        entity.HasIndex(item => item.ExternalId).IsUnique(); entity.Property(item => item.Revision).IsConcurrencyToken();
        entity.HasOne(item => item.Resource).WithMany().HasForeignKey(item => item.ResourceId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(item => item.OpenKey).HasMaxLength(32);
        entity.HasIndex(item => item.OpenKey).IsUnique().HasFilter("openkey IS NOT NULL");
        var change = builder.Entity<IncidentChange>(); change.ToTable("incidentchange"); change.HasKey(item => item.Id);
        change.HasIndex(item => new { item.IncidentId, item.Revision }).IsUnique();
        change.HasOne(item => item.Incident).WithMany(item => item.Changes).HasForeignKey(item => item.IncidentId).OnDelete(DeleteBehavior.Restrict);
        change.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        entity.Property(item => item.Title).HasMaxLength(120).IsRequired();
        change.Property(item => item.Title).HasMaxLength(120).IsRequired();
        entity.Property(item => item.Impact).HasMaxLength(1000).IsRequired();
        change.Property(item => item.Impact).HasMaxLength(1000).IsRequired();
        entity.Property(item => item.Owner).HasMaxLength(120).IsRequired();
        change.Property(item => item.Owner).HasMaxLength(120).IsRequired();
        entity.Property(item => item.NextAction).HasMaxLength(500).IsRequired();
        change.Property(item => item.NextAction).HasMaxLength(500).IsRequired();
        entity.Property(item => item.InternalNote).HasMaxLength(3000).IsRequired();
        change.Property(item => item.InternalNote).HasMaxLength(3000).IsRequired();
        entity.Property(item => item.Severity).HasMaxLength(20).IsRequired();
        change.Property(item => item.Severity).HasMaxLength(20).IsRequired();
        entity.Property(item => item.Status).HasMaxLength(20).IsRequired();
        change.Property(item => item.Status).HasMaxLength(20).IsRequired();
    }
}
