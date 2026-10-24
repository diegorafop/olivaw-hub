using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class UsageReadingExtensions
{
    internal static void SetupUsageReading(this ModelBuilder builder)
    {
        var entity = builder.Entity<UsageReading>(); entity.ToTable("usagereading"); entity.HasKey(item => item.Id);
        entity.HasIndex(item => item.ExternalId).IsUnique();
        entity.HasIndex(item => new { item.InventoryResourceId, item.SampleKey }).IsUnique();
        entity.HasOne(item => item.InventoryResource).WithMany().HasForeignKey(item => item.InventoryResourceId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        entity.Property(item => item.Source).HasMaxLength(300).IsRequired();
        entity.Property(item => item.Note).HasMaxLength(1000).IsRequired();
        entity.Property(item => item.Metric).HasMaxLength(30).IsRequired();
        entity.Property(item => item.Value).HasPrecision(18, 3);
        entity.Property(item => item.Quality).HasMaxLength(20).IsRequired();
    }
}
