using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Owh.Olivaw.Dados;
using Owh.Olivaw.Dados.Repositories;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.InventoryFeatureTest;
public sealed class InventoryMappingTest
{
    [Theory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public void Model_should_preserve_concurrency_identity_period_indexes_and_translate_queries(string provider)
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>();
        if (provider == "PostgreSql") options.UseNpgsql("Host=localhost;Database=unused");
        else options.UseSqlServer("Server=localhost;Database=unused;Integrated Security=true;");
        options.UseLowerCaseNamingConvention();
        using var context = new WriteDbContext(options.Options, new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["DatabaseProvider"] = provider }).Build());
        var repository = new RepositoryInventoryResource(context);
        var sql = repository.ReadRows(new() { Search = "vm", Type = "compute", Provider = "AWS", Status = "verified" }).OrderBy(item => item.Id).Skip(20).Take(20).ToQueryString();
        Assert.Contains("OFFSET", sql, StringComparison.OrdinalIgnoreCase);
        foreach (var kind in new[] { "customer", "dependency", "deployment" })
        {
            var links = repository.ReadLinkRows(new InventoryLinkFilter { ResourceExternalId = Guid.NewGuid(), Kind = kind, Page = 2, PageSize = 20 }).OrderBy(item => item.StartedAt).Skip(20).Take(20).ToQueryString();
            Assert.Contains("endedat", links); Assert.Contains("OFFSET", links, StringComparison.OrdinalIgnoreCase);
        }
        foreach (var type in new[] { typeof(ResourceCustomer), typeof(ResourceDependency), typeof(DeploymentResource), typeof(ServiceDeployment) })
        {
            var mapped = context.Model.FindEntityType(type)!;
            Assert.True(mapped.FindProperty("Revision")!.IsConcurrencyToken);
            Assert.Contains(mapped.GetIndexes(), index => index.IsUnique && index.GetFilter() == "endedat IS NULL");
            Assert.All(mapped.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        }
        var resource = context.Model.FindEntityType(typeof(InventoryResource))!;
        Assert.Contains(resource.GetIndexes(), index => index.IsUnique && index.GetFilter() == "provideridentitykey IS NOT NULL");
        Assert.Equal(3, resource.FindProperty("MemoryCapacityGiB")!.GetScale());
        Assert.True(context.Model.FindEntityType(typeof(InventoryRevision))!.FindProperty("Revision")!.IsConcurrencyToken);
        foreach (var type in new[] { typeof(InventoryResource), typeof(InventoryResourceChange) })
        {
            var details = context.Model.FindEntityType(type)!;
            Assert.Equal(80, details.FindProperty("Technology")!.GetMaxLength());
            Assert.Equal(40, details.FindProperty("TechnologyVersion")!.GetMaxLength());
            Assert.Equal(120, details.FindProperty("LogicalName")!.GetMaxLength());
            Assert.Equal(typeof(DateTime?), details.FindProperty("RenewalAt")!.ClrType);
            Assert.Equal(typeof(DateTime?), details.FindProperty("CertificateExpiresAt")!.ClrType);
            Assert.Equal(typeof(int?), details.FindProperty("BackupIntervalHours")!.ClrType);
            Assert.Equal(typeof(int?), details.FindProperty("BackupRetentionDays")!.ClrType);
        }
        var source = InventoryFakerBuilder.Resource(); var target = InventoryFakerBuilder.Deployment();
        source.Changes.Clear(); target.Changes.Clear(); target.ManagedService.Changes.Clear();
        context.Attach(source); context.Attach(target);
        var link = DeploymentResource.Create(source, target, "Uso", 1, 2, 3, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        context.Add(link); context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Unchanged, context.Entry(source).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(target).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(target.ManagedService).State);
        Assert.Equal(EntityState.Added, context.Entry(link).State);
    }
}
