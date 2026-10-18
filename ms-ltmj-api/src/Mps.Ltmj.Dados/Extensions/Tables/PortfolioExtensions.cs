using Microsoft.EntityFrameworkCore;
using Mps.Ltmj.Domain;

namespace Mps.Ltmj.Dados.Extensions.Tables;

internal static class PortfolioExtensions
{
    internal static void SetupPortfolio(this ModelBuilder builder)
    {
        var entity = builder.Entity<PortfolioEvent>();
        entity.ToTable("portfolio_events");
        entity.HasIndex(item => item.IdExterno).IsUnique();
        entity.Property(item => item.Title).HasMaxLength(150).IsRequired();
        entity.Property(item => item.Description).HasMaxLength(4000).IsRequired();
        entity.Property(item => item.Category).HasMaxLength(40).IsRequired();
        entity.OwnsMany(item => item.Photos, photo =>
        {
            photo.ToTable("portfolio_photos");
            photo.HasKey(item => item.Id);
            photo.Property(item => item.StorageKey).HasMaxLength(80).IsRequired();
            photo.Property(item => item.ContentType).HasMaxLength(30).IsRequired();
        });
    }
}
