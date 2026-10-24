using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Owh.Olivaw.Dados;
using Owh.Olivaw.Dados.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.LeadFeatureTest;
public class RepositoryLeadSqlTest
{
    [Theory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public void Query_should_translate_json_filters_and_pagination_without_database_connection(string provider)
    {
        var builder = new DbContextOptionsBuilder<WriteDbContext>();
        if (provider == "PostgreSql") builder.UseNpgsql("Host=localhost;Database=unused");
        else builder.UseSqlServer("Server=localhost;Database=unused;Integrated Security=true;");
        builder.UseLowerCaseNamingConvention();
        using var context = new WriteDbContext(builder.Options, new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["DatabaseProvider"] = provider }).Build());
        var term = "pastelaria";
        var sql = new RepositoryLead(context).ReadRows().Where(item => item.Business.ToLower().Contains(term) && item.Entry == "studio" && item.FollowUpStatus == "analysis")
            .OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.ExternalId).Skip(20).Take(20).ToQueryString();
        Assert.Contains("deletedat IS NULL", sql);
        Assert.Contains("FollowUpStatus", sql);
        var lead = context.Model.FindEntityType(typeof(Owh.Olivaw.Domain.Lead))!;
        Assert.True(lead.FindProperty("FollowUpRevision")!.IsConcurrencyToken);
        Assert.Equal("new", lead.FindProperty("FollowUpStatus")!.GetDefaultValue());
        var change = context.Model.FindEntityType(typeof(Owh.Olivaw.Domain.LeadChange))!;
        Assert.Equal("leadchange", change.GetTableName());
        Assert.Contains(change.GetIndexes(), item => item.IsUnique && item.Properties.Select(property => property.Name).SequenceEqual(new[] { "LeadId", "Revision" }));
        Assert.Equal(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict, Assert.Single(change.GetForeignKeys()).DeleteBehavior);
        Assert.Contains("OFFSET", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("@term", sql);
        Assert.DoesNotContain("writekeyhash", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("actoruserid", sql, StringComparison.OrdinalIgnoreCase);
        if (provider == "PostgreSql") Assert.Contains("snapshot::jsonb", sql);
        else Assert.Contains("JSON_VALUE", sql);
    }
}
