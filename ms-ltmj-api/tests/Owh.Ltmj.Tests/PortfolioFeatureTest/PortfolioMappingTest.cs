using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using Owh.Ltmj.Dados;
using Owh.Ltmj.Domain;

namespace Owh.Ltmj.Tests.PortfolioFeatureTest;

public class PortfolioMappingTest
{
    [Theory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public void Model_should_map_event_and_related_photos_for_supported_providers(string provider)
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>();
        if (provider == "PostgreSql") options.UseNpgsql("Host=localhost;Database=unused");
        else options.UseSqlServer("Server=localhost;Database=unused;TrustServerCertificate=true");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["DatabaseProvider"] = provider }).Build();
        using var context = new WriteDbContext(options.Options, config);
        var entity = context.Model.FindEntityType(typeof(PortfolioEvent));
        Assert.NotNull(entity);
        Assert.Equal(150, entity.FindProperty(nameof(PortfolioEvent.Title))!.GetMaxLength());
        var photos = entity.FindNavigation(nameof(PortfolioEvent.Photos))!.TargetEntityType;
        Assert.False(photos.IsOwned());
        Assert.Equal(typeof(int), photos.FindProperty(nameof(PortfolioPhoto.Id))!.ClrType);
        Assert.Equal(typeof(int), photos.FindProperty(nameof(PortfolioPhoto.PortfolioEventId))!.ClrType);
        Assert.Equal(entity, photos.FindNavigation(nameof(PortfolioPhoto.PortfolioEvent))!.ForeignKey.PrincipalEntityType);
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && index.Properties.Any(property => property.Name == nameof(PortfolioEvent.ExternalId)));
    }
}
