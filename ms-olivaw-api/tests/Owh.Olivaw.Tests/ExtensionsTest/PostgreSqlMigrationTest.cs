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
}
