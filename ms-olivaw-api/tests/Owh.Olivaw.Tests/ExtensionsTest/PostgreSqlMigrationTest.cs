using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Dados.PostgreSql;
using Xunit;

namespace Owh.Olivaw.Tests.ExtensionsTest;

public sealed class PostgreSqlMigrationTest
{
    [Fact]
    public void Model_should_match_the_latest_postgresql_migration()
    {
        using var context = new WriteDbContextFactory().CreateDbContext([]);

        Assert.False(context.Database.HasPendingModelChanges(),
            "O modelo PostgreSQL mudou sem migration. O startup da API rejeitaria a publicação.");
    }
    [Fact]
    public void Portal_model_should_match_its_own_migration_and_keep_authorities_separate()
    {
        using var context = new PortalIdentityDbContextFactory().CreateDbContext([]);
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.All(context.Model.GetEntityTypes(), entity => Assert.StartsWith("portal", entity.GetTableName()));
        Assert.Single(context.Database.GetMigrations());
        var sql = context.Database.GenerateCreateScript();
        Assert.Contains("portaluserprofile", sql);
        Assert.DoesNotContain("CREATE TABLE userprofile ", sql);
    }
}
