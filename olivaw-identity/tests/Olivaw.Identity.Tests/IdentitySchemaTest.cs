using Microsoft.EntityFrameworkCore;
using Olivaw.Identity.Domain;
using Xunit;

namespace Olivaw.Identity.Tests;

public class IdentitySchemaTest
{
    [Fact]
    public void Model_should_use_english_tables_and_int_primary_and_foreign_keys()
    {
        using var context = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite("Data Source=:memory:").Options);
        var entities = context.Model.GetEntityTypes().ToArray();
        Assert.Equal(new[] { "accessprofile", "accessprofileauthorization", "authorization", "userprofile", "userprofileaccessprofile" },
            entities.Select(entity => entity.GetTableName()).OrderBy(name => name, StringComparer.Ordinal));
        foreach (var entity in entities)
        {
            var key = Assert.Single(entity.GetKeys());
            Assert.Equal(typeof(int), Assert.Single(key.Properties).ClrType);
            Assert.Equal(typeof(Guid), entity.FindProperty(nameof(UserProfile.ExternalId))?.ClrType);
            Assert.NotNull(entity.FindProperty(nameof(UserProfile.CreatedAt)));
            Assert.NotNull(entity.FindProperty(nameof(UserProfile.ActorUserId)));
            foreach (var relation in entity.GetForeignKeys())
            {
                Assert.All(relation.Properties, property => Assert.Equal(typeof(int), property.ClrType));
                Assert.All(relation.PrincipalKey.Properties, property => Assert.Equal(typeof(int), property.ClrType));
            }
        }
    }
}
