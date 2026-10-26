using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class PortalUsageExtensions
{
    internal static void SetupPortalUsage(this ModelBuilder builder)
    {
        var e=builder.Entity<PortalUsageSample>();e.ToTable("portalusagesample");e.HasKey(x=>x.Id);
        e.HasIndex(x=>x.ExternalId).IsUnique();e.HasIndex(x=>x.SampleKey).IsUnique();e.HasIndex(x=>new{x.CustomerId,x.Day}).IsUnique();
        e.Property(x=>x.StorageUsedMiB).HasPrecision(18,3);e.Property(x=>x.Source).HasMaxLength(300).IsRequired();
        e.HasOne(x=>x.Customer).WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
