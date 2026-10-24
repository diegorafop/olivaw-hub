using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;

namespace Owh.Olivaw.Dados.Extensions.Tables;

internal static class CustomerExtensions
{
    internal static void SetupCustomer(this ModelBuilder builder)
    {
        var customer = builder.Entity<Customer>();
        customer.ToTable("customer");
        customer.HasKey(item => item.Id);
        customer.HasIndex(item => item.ExternalId).IsUnique();
        customer.HasIndex(item => item.Document).IsUnique().HasFilter("document IS NOT NULL");
        customer.HasIndex(item => item.SourceLeadId).IsUnique().HasFilter("sourceleadid IS NOT NULL");
        customer.HasIndex(item => item.Status);
        customer.Property(item => item.Business).HasMaxLength(120).IsRequired();
        customer.Property(item => item.LegalName).HasMaxLength(180).IsRequired();
        customer.Property(item => item.Document).HasMaxLength(14);
        customer.Property(item => item.Owner).HasMaxLength(120).IsRequired();
        customer.Property(item => item.Notes).HasMaxLength(3000).IsRequired();
        customer.Property(item => item.Status).HasMaxLength(20).IsRequired();
        customer.Property(item => item.Revision).IsConcurrencyToken();
        customer.HasOne(item => item.SourceLead).WithMany().HasForeignKey(item => item.SourceLeadId).OnDelete(DeleteBehavior.Restrict);
        customer.HasMany(item => item.Contacts).WithOne(item => item.Customer).HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Cascade);
        customer.HasMany(item => item.Changes).WithOne(item => item.Customer).HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Cascade);
        var contact = builder.Entity<CustomerContact>();
        contact.ToTable("customercontact");
        contact.HasKey(item => item.Id);
        contact.HasIndex(item => item.ExternalId).IsUnique();
        contact.Property(item => item.Name).HasMaxLength(120).IsRequired();
        contact.Property(item => item.Role).HasMaxLength(120).IsRequired();
        contact.Property(item => item.Email).HasMaxLength(254).IsRequired();
        contact.Property(item => item.Whatsapp).HasMaxLength(11).IsRequired();
        var change = builder.Entity<CustomerChange>();
        change.ToTable("customerchange");
        change.HasKey(item => item.Id);
        change.HasIndex(item => new { item.CustomerId, item.CreatedAt });
        change.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        change.Property(item => item.Action).HasMaxLength(20).IsRequired();
        change.Property(item => item.PreviousStatus).HasMaxLength(20).IsRequired();
        change.Property(item => item.Status).HasMaxLength(20).IsRequired();
    }
}
