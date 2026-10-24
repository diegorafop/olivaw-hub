using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class SupportRequestExtensions
{
    internal static void SetupSupportRequest(this ModelBuilder builder)
    {
        var entity = builder.Entity<SupportRequest>(); entity.ToTable("supportrequest"); entity.HasKey(item => item.Id);
        entity.Property(item => item.Protocol).HasMaxLength(32).IsRequired(); entity.HasIndex(item => item.Protocol).IsUnique();
        entity.HasIndex(item => new { item.CustomerId, item.PortalSubmissionKey }).IsUnique().HasFilter("portalsubmissionkey IS NOT NULL");
        entity.Property(item => item.SubmissionDigest).HasMaxLength(64).IsRequired(); entity.Property(item => item.RequestType).HasMaxLength(20).IsRequired(); entity.Property(item => item.DesiredPlan).HasMaxLength(120).IsRequired();
        entity.Property(item => item.DesiredStorageGB).HasPrecision(18, 3); entity.Property(item => item.DesiredMemoryMiB).HasPrecision(18, 3);
        entity.HasOne(item => item.ManagedService).WithMany().HasForeignKey(item => item.ManagedServiceId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(item => item.PublicSummary).HasMaxLength(1000).IsRequired();
        entity.HasIndex(item => item.ExternalId).IsUnique(); entity.Property(item => item.Revision).IsConcurrencyToken();
        entity.HasOne(item => item.Customer).WithMany().HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Restrict);
        var message = builder.Entity<SupportMessage>(); message.ToTable("supportmessage"); message.HasKey(item => item.Id);
        message.HasIndex(item => item.ExternalId).IsUnique(); message.HasIndex(item => new { item.SupportRequestId, item.MessageKey }).IsUnique();
        message.HasOne(item => item.SupportRequest).WithMany().HasForeignKey(item => item.SupportRequestId).OnDelete(DeleteBehavior.Restrict);
        message.Property(item => item.Body).HasMaxLength(3000).IsRequired(); message.Property(item => item.AuthorName).HasMaxLength(120).IsRequired();
        var change = builder.Entity<SupportRequestChange>(); change.ToTable("supportrequestchange"); change.HasKey(item => item.Id);
        change.HasIndex(item => new { item.SupportRequestId, item.Revision }).IsUnique();
        change.HasOne(item => item.SupportRequest).WithMany(item => item.Changes).HasForeignKey(item => item.SupportRequestId).OnDelete(DeleteBehavior.Restrict);
        change.Property(item => item.PublicSummary).HasMaxLength(1000).IsRequired(); change.Property(item => item.ServiceLabel).HasMaxLength(120).IsRequired();
        change.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        entity.Property(item => item.Title).HasMaxLength(120).IsRequired();
        change.Property(item => item.Title).HasMaxLength(120).IsRequired();
        entity.Property(item => item.Requester).HasMaxLength(120).IsRequired();
        change.Property(item => item.Requester).HasMaxLength(120).IsRequired();
        entity.Property(item => item.Impact).HasMaxLength(1000).IsRequired();
        change.Property(item => item.Impact).HasMaxLength(1000).IsRequired();
        entity.Property(item => item.Owner).HasMaxLength(120).IsRequired();
        change.Property(item => item.Owner).HasMaxLength(120).IsRequired();
        entity.Property(item => item.NextAction).HasMaxLength(500).IsRequired();
        change.Property(item => item.NextAction).HasMaxLength(500).IsRequired();
        entity.Property(item => item.InternalNote).HasMaxLength(3000).IsRequired();
        change.Property(item => item.InternalNote).HasMaxLength(3000).IsRequired();
        entity.Property(item => item.Category).HasMaxLength(20).IsRequired();
        change.Property(item => item.Category).HasMaxLength(20).IsRequired();
        entity.Property(item => item.Status).HasMaxLength(20).IsRequired();
        change.Property(item => item.Status).HasMaxLength(20).IsRequired();
    }
}
