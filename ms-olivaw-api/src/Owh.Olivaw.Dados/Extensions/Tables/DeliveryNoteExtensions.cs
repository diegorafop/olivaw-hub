using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class DeliveryNoteExtensions
{
    internal static void SetupDeliveryNotes(this ModelBuilder builder)
    {
        var note = builder.Entity<DeliveryNote>(); note.ToTable("deliverynote"); note.HasKey(item => item.Id); note.HasIndex(item => item.ExternalId).IsUnique(); note.HasIndex(item => new { item.CustomerId, item.DeliveredAt, item.Id }); note.Property(item => item.Revision).IsConcurrencyToken();
        note.HasOne(item => item.Customer).WithMany().HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Restrict);
        note.HasOne(item => item.ManagedService).WithMany().HasForeignKey(item => item.ManagedServiceId).OnDelete(DeleteBehavior.Restrict);
        note.HasOne(item => item.SupportRequest).WithMany().HasForeignKey(item => item.SupportRequestId).OnDelete(DeleteBehavior.Restrict);
        var change = builder.Entity<DeliveryNoteChange>(); change.ToTable("deliverynotechange"); change.HasKey(item => item.Id); change.HasIndex(item => new { item.DeliveryNoteId, item.Revision }).IsUnique();
        change.HasOne(item => item.DeliveryNote).WithMany(item => item.Changes).HasForeignKey(item => item.DeliveryNoteId).OnDelete(DeleteBehavior.Restrict); change.Property(item => item.AuthorName).HasMaxLength(120).IsRequired();
        note.Property(item => item.Title).HasMaxLength(120).IsRequired();
        change.Property(item => item.Title).HasMaxLength(120).IsRequired();
        note.Property(item => item.Summary).HasMaxLength(3000).IsRequired();
        change.Property(item => item.Summary).HasMaxLength(3000).IsRequired();
        note.Property(item => item.InternalNote).HasMaxLength(3000).IsRequired();
        change.Property(item => item.InternalNote).HasMaxLength(3000).IsRequired();
        note.Property(item => item.Version).HasMaxLength(80).IsRequired();
        change.Property(item => item.Version).HasMaxLength(80).IsRequired();
        note.Property(item => item.Category).HasMaxLength(20).IsRequired();
        change.Property(item => item.Category).HasMaxLength(20).IsRequired();
        note.Property(item => item.Status).HasMaxLength(20).IsRequired();
        change.Property(item => item.Status).HasMaxLength(20).IsRequired();
        note.Property(item => item.ApprovalReference).HasMaxLength(300).IsRequired();
        change.Property(item => item.ApprovalReference).HasMaxLength(300).IsRequired();
        note.Property(item => item.ServiceLabel).HasMaxLength(120).IsRequired();
        change.Property(item => item.ServiceLabel).HasMaxLength(120).IsRequired();
    }
}
