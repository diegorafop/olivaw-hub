using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class PortalExtensions
{
    internal static void SetupPortal(this ModelBuilder builder)
    {
        var audit = builder.Entity<PortalAccessAudit>(); audit.ToTable("portalaccessaudit"); audit.HasKey(item => item.Id);
        audit.HasIndex(item => item.ExternalId).IsUnique(); audit.Property(item => item.Action).HasMaxLength(20).IsRequired(); audit.Property(item => item.Details).HasMaxLength(1000).IsRequired(); audit.Property(item => item.ActorAuthority).HasMaxLength(20).IsRequired(); audit.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        audit.HasOne(item => item.Customer).WithMany().HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Restrict);
        var link = builder.Entity<PortalServiceLink>(); link.ToTable("portalservicelink"); link.HasKey(item => item.Id);
        link.HasIndex(item => item.ExternalId).IsUnique(); link.HasIndex(item => new { item.CustomerId, item.ManagedServiceId }).IsUnique();
        link.Property(item => item.Label).HasMaxLength(120).IsRequired(); link.Property(item => item.PublicUrl).HasMaxLength(300).IsRequired(); link.Property(item => item.Revision).IsConcurrencyToken();
        link.HasOne(item => item.Customer).WithMany().HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Restrict);
        link.HasOne(item => item.ManagedService).WithMany().HasForeignKey(item => item.ManagedServiceId).OnDelete(DeleteBehavior.Restrict);
        link.HasOne(item => item.HealthResource).WithMany().HasForeignKey(item => item.HealthResourceId).OnDelete(DeleteBehavior.Restrict);
        var member = builder.Entity<PortalMembership>(); member.ToTable("portalmembership"); member.HasKey(item => item.Id);
        member.HasIndex(item => item.ExternalId).IsUnique(); member.HasIndex(item => new { item.PortalUserExternalId, item.CustomerId }).IsUnique();
        member.HasOne(item => item.Customer).WithMany().HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Restrict);
        member.Property(item => item.Role).HasMaxLength(20).IsRequired(); member.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        member.Property(item => item.Revision).IsConcurrencyToken(); member.Ignore(item => item.CanRequest); member.Ignore(item => item.CanReadBilling);
        var invite = builder.Entity<PortalInvitation>(); invite.ToTable("portalinvitation"); invite.HasKey(item => item.Id);
        invite.HasIndex(item => item.ExternalId).IsUnique(); invite.HasIndex(item => item.TokenHash).IsUnique();
        invite.HasOne(item => item.Customer).WithMany().HasForeignKey(item => item.CustomerId).OnDelete(DeleteBehavior.Restrict);
        invite.Property(item => item.Email).HasMaxLength(254).IsRequired(); invite.Property(item => item.Role).HasMaxLength(20).IsRequired();
        invite.Property(item => item.TokenHash).HasMaxLength(64).IsRequired(); invite.Property(item => item.Revision).IsConcurrencyToken();
    }
}
