using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using Mps.Ltmj.Dados;
using Mps.Ltmj.Domain;

namespace Mps.Ltmj.Tests.PortfolioFeatureTest;

public class PortfolioMappingTest
{
    [Theory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public void Model_should_map_event_and_owned_photos_for_supported_providers(string provider)
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>();
        if (provider == "PostgreSql") options.UseNpgsql("Host=localhost;Database=unused");
        else options.UseSqlServer("Server=localhost;Database=unused;TrustServerCertificate=true");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["DatabaseProvider"] = provider }).Build();
        using var context = new WriteDbContext(options.Options, config);
        var entity = context.Model.FindEntityType(typeof(PortfolioEvent));
        Assert.NotNull(entity);
        Assert.Equal(150, entity.FindProperty(nameof(PortfolioEvent.Title))!.GetMaxLength());
        Assert.True(entity.FindNavigation(nameof(PortfolioEvent.Photos))!.TargetEntityType.IsOwned());
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && index.Properties.Any(property => property.Name == nameof(PortfolioEvent.IdExterno)));
    }
}
