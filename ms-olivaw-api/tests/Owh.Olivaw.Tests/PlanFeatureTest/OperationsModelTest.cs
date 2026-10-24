using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Owh.Olivaw.Dados;
using Owh.Olivaw.Domain;
using Xunit;
namespace Owh.Olivaw.Tests.PlanFeatureTest;
public sealed class OperationsModelTest
{
    [Theory]
    [InlineData("PostgreSql")] [InlineData("SqlServer")]
    public void Model_should_map_typed_history_and_translate_scoped_queries_without_connecting(string provider)
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>();
        if (provider == "PostgreSql") options.UseNpgsql("Host=localhost;Database=unused"); else options.UseSqlServer("Server=localhost;Database=unused;");
        options.UseLowerCaseNamingConvention();
        using var context = new WriteDbContext(options.Options, new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["DatabaseProvider"] = provider }).Build());
        var sql = context.Set<PlanStudyCost>().Where(item => new[] { 1, 2 }.Contains(item.PlanStudyId)).Include(item => item.CostReference).ToQueryString();
        Assert.Contains("costreference", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, context.Model.FindEntityType(typeof(CostReference))!.FindProperty("MonthlyCost")!.GetScale());
        Assert.Equal(3, context.Model.FindEntityType(typeof(PlanStudy))!.FindProperty("StorageLimitGB")!.GetScale());
        foreach (var type in new[] { typeof(HealthReading), typeof(UsageReading), typeof(CareEvidence) })
        {
            var entity = context.Model.FindEntityType(type)!;
            Assert.Contains(entity.GetIndexes(), item => item.IsUnique && item.Properties.Any(property => property.Name == "SampleKey"));
            Assert.All(entity.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        }
        Assert.Contains(context.Model.FindEntityType(typeof(Incident))!.GetIndexes(), item => item.IsUnique && item.GetFilter() == "openkey IS NOT NULL");
        Assert.True(context.Model.FindEntityType(typeof(SupportRequest))!.FindProperty("Revision")!.IsConcurrencyToken);
        Assert.Contains(context.Model.FindEntityType(typeof(IncidentChange))!.GetIndexes(), item => item.IsUnique && item.Properties.Any(property => property.Name == "Revision"));
    }
}
