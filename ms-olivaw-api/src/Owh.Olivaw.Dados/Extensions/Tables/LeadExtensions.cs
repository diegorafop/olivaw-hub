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
    }
}
