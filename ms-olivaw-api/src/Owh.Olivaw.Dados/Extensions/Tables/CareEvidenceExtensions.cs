using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class CareEvidenceExtensions
{
    internal static void SetupCareEvidence(this ModelBuilder builder)
    {
        var entity = builder.Entity<CareEvidence>(); entity.ToTable("careevidence"); entity.HasKey(item => item.Id);
        entity.HasIndex(item => item.ExternalId).IsUnique();
        entity.HasIndex(item => new { item.InventoryResourceId, item.SampleKey }).IsUnique();
        entity.HasOne(item => item.InventoryResource).WithMany().HasForeignKey(item => item.InventoryResourceId).OnDelete(DeleteBehavior.Restrict);
        entity.Property(item => item.ActorName).HasMaxLength(120).IsRequired();
        entity.Property(item => item.Source).HasMaxLength(300).IsRequired();
        entity.Property(item => item.Note).HasMaxLength(1000).IsRequired();
        entity.Property(item => item.Kind).HasMaxLength(20).IsRequired();
        entity.Property(item => item.Outcome).HasMaxLength(20).IsRequired();
        entity.Property(item => item.ReferenceUrl).HasMaxLength(300).IsRequired();
    }
}
