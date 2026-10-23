using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Owh.Olivaw.Dados;
using Owh.Olivaw.Dados.Repositories;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;

namespace Owh.Olivaw.Tests.ManagedServiceFeatureTest;

public class ManagedServiceMappingTest
{
    [Theory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public void Model_should_translate_filters_and_preserve_unique_identity_and_link_periods(string provider)
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>();
        if (provider == "PostgreSql") options.UseNpgsql("Host=localhost;Database=unused");
        else options.UseSqlServer("Server=localhost;Database=unused;Integrated Security=true;");
        options.UseLowerCaseNamingConvention();
        using var context = new WriteDbContext(options.Options, new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["DatabaseProvider"] = provider }).Build());
        var sql = new RepositoryManagedService(context).ReadRows("api", "active", "api", Guid.NewGuid()).OrderBy(item => item.Id).Skip(20).Take(20).ToQueryString();
        Assert.Contains("OFFSET", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("endedat", sql);
        var service = context.Model.FindEntityType(typeof(ManagedService))!;
        Assert.True(service.FindProperty(nameof(ManagedService.Revision))!.IsConcurrencyToken);
        Assert.Contains(service.GetIndexes(), index => index.IsUnique && index.Properties.Single().Name == "TechnicalId");
        var link = context.Model.FindEntityType(typeof(ManagedServiceCustomer))!;
        Assert.Contains(link.GetIndexes(), index => index.IsUnique && index.GetFilter() == "endedat IS NULL");
        Assert.All(link.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        var customer = ManagedServiceFakerBuilder.Customer();
        var entity = ManagedServiceFakerBuilder.Create("customer");
        context.Attach(customer);
        entity.Customers.Add(ManagedServiceCustomer.Create(entity, customer, ManagedServiceFakerBuilder.Actor, "Operador", ManagedServiceFakerBuilder.Now));
        context.Add(entity);
        Assert.Equal(EntityState.Unchanged, context.Entry(customer).State);
        Assert.Equal(EntityState.Added, context.Entry(Assert.Single(entity.Customers)).State);
        Assert.Equal(EntityState.Added, context.Entry(Assert.Single(entity.Changes)).State);
        context.ChangeTracker.Clear();
        var existing = ManagedServiceFakerBuilder.Create("customer");
        existing.Id = 10;
        existing.Changes.Clear();
        context.Attach(existing);
        context.Attach(customer);
        var change = existing.Apply("Atualizado", "api", "", "", "customer", "active", ManagedServiceFakerBuilder.Actor, "Operador", ManagedServiceFakerBuilder.Now);
        existing.Customers.Add(ManagedServiceCustomer.Create(existing, customer, ManagedServiceFakerBuilder.Actor, "Operador", ManagedServiceFakerBuilder.Now));
        context.Add(change);
        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, context.Entry(existing).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(customer).State);
        Assert.Equal(1L, context.Entry(existing).Property(item => item.Revision).OriginalValue);
        Assert.Equal(2L, context.Entry(existing).Property(item => item.Revision).CurrentValue);
        Assert.Equal(EntityState.Added, context.Entry(Assert.Single(existing.Customers)).State);
    }
}
