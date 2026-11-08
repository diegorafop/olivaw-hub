using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;

namespace Owh.Olivaw.Dados.Extensions.Tables;

internal static class LeadExtensions
{
    internal static void SetupLead(this ModelBuilder builder)
    {
        var entity = builder.Entity<Lead>();
        entity.ToTable("lead");
        entity.HasKey(item => item.Id);
        entity.HasIndex(item => item.ExternalId).IsUnique();
        entity.Property(item => item.WriteKeyHash).HasMaxLength(64).IsRequired();
        entity.Property(item => item.Snapshot).HasMaxLength(32000).IsRequired();
        entity.Property(item => item.Stage).HasMaxLength(20).IsRequired();
        entity.Property(item => item.Revision).IsConcurrencyToken();
        entity.Property(item => item.FollowUpStatus).HasMaxLength(20).HasDefaultValue("new").IsRequired();
        entity.Property(item => item.FollowUpRevision).HasDefaultValue(0L).IsConcurrencyToken();
        entity.HasIndex(item => item.FollowUpStatus);
        var change = builder.Entity<LeadChange>();
        change.ToTable("leadchange");
        change.HasKey(item => item.Id);
        change.HasIndex(item => item.ExternalId).IsUnique();
        change.HasIndex(item => new { item.LeadId, item.Revision }).IsUnique();
        change.HasOne(item => item.Lead).WithMany().HasForeignKey(item => item.LeadId).OnDelete(DeleteBehavior.Restrict);
        change.Property(item => item.PreviousStatus).HasMaxLength(20).IsRequired();
        change.Property(item => item.Status).HasMaxLength(20).IsRequired();
        change.Property(item => item.Note).HasMaxLength(3000).IsRequired();
        change.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
    }
}
