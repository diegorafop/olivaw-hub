using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;

namespace Owh.Olivaw.Dados.Extensions.Tables;

internal static class ManagedServiceExtensions
{
    internal static void SetupManagedService(this ModelBuilder builder)
    {
        var service = builder.Entity<ManagedService>();
        service.ToTable("managedservice");
        service.HasKey(item => item.Id);
        service.HasIndex(item => item.ExternalId).IsUnique();
        service.HasIndex(item => item.TechnicalId).IsUnique();
        service.Property(item => item.TechnicalId).HasMaxLength(80).IsRequired();
        service.Property(item => item.Name).HasMaxLength(120).IsRequired();
        service.Property(item => item.Type).HasMaxLength(20).IsRequired();
        service.Property(item => item.Scope).HasMaxLength(20).IsRequired();
        service.Property(item => item.Status).HasMaxLength(20).IsRequired();
        service.Property(item => item.Description).HasMaxLength(1000).IsRequired();
        service.Property(item => item.Owner).HasMaxLength(120).IsRequired();
        service.Property(item => item.Revision).IsConcurrencyToken();
        service.HasMany(item => item.Customers).WithOne(item => item.ManagedService).HasForeignKey(item => item.ManagedServiceId).OnDelete(DeleteBehavior.Restrict);
        var link = builder.Entity<ManagedServiceCustomer>();
        link.ToTable("managedservicecustomer");
        link.HasKey(item => item.Id);
        link.Property(item => item.StartedByName).HasMaxLength(120).IsRequired();
        link.Property(item => item.EndedByName).HasMaxLength(120);
        link.HasIndex(item => item.ExternalId).IsUnique();
        link.HasIndex(item => new { item.ManagedServiceId, item.CustomerId }).IsUnique().HasFilter("endedat IS NULL");
        link.HasOne(item => item.Customer).WithMany().HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Restrict);
        var change = builder.Entity<ManagedServiceChange>();
        change.ToTable("managedservicechange");
        change.HasKey(item => item.Id);
        change.HasIndex(item => new { item.ManagedServiceId, item.Revision }).IsUnique();
        change.HasOne(item => item.ManagedService).WithMany(item => item.Changes).HasForeignKey(item => item.ManagedServiceId).OnDelete(DeleteBehavior.Restrict);
        change.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        change.Property(item => item.PreviousStatus).HasMaxLength(20).IsRequired();
        change.Property(item => item.Status).HasMaxLength(20).IsRequired();
        change.Property(item => item.Name).HasMaxLength(120).IsRequired();
        change.Property(item => item.Type).HasMaxLength(20).IsRequired();
        change.Property(item => item.Scope).HasMaxLength(20).IsRequired();
        change.Property(item => item.Description).HasMaxLength(1000).IsRequired();
        change.Property(item => item.Owner).HasMaxLength(120).IsRequired();
    }
}
