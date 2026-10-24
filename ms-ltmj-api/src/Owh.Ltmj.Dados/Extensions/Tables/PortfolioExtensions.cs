using Microsoft.EntityFrameworkCore;
using Owh.Ltmj.Domain;

namespace Owh.Ltmj.Dados.Extensions.Tables;

internal static class PortfolioExtensions
{
    internal static void SetupPortfolio(this ModelBuilder builder)
    {
        var entity = builder.Entity<PortfolioEvent>();
        entity.ToTable("portfolioevent");
        entity.HasIndex(item => item.ExternalId).IsUnique();
        entity.Property(item => item.Title).HasMaxLength(150).IsRequired();
        entity.Property(item => item.Description).HasMaxLength(4000).IsRequired();
        entity.Property(item => item.Category).HasMaxLength(40).IsRequired();
        entity.HasMany(item => item.Photos)
            .WithOne(photo => photo.PortfolioEvent)
            .HasForeignKey(photo => photo.PortfolioEventId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        entity.Navigation(item => item.Photos).AutoInclude();

        var photo = builder.Entity<PortfolioPhoto>();
        photo.ToTable("portfoliophoto");
        photo.HasKey(item => item.Id);
        photo.HasIndex(item => item.ExternalId).IsUnique();
        photo.Property(item => item.StorageKey).HasMaxLength(80).IsRequired();
        photo.Property(item => item.ContentType).HasMaxLength(30).IsRequired();
    }
}
