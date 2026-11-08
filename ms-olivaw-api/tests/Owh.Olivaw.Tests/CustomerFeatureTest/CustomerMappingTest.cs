using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Owh.Olivaw.Dados;
using Owh.Olivaw.Domain;
using Xunit;

namespace Owh.Olivaw.Tests.CustomerFeatureTest;
public class CustomerMappingTest
{
    [Theory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public void Model_should_map_relational_contacts_history_and_unique_origin_without_connecting(string provider)
    {
        var builder = new DbContextOptionsBuilder<WriteDbContext>();
        if (provider == "PostgreSql") builder.UseNpgsql("Host=localhost;Database=unused");
        else builder.UseSqlServer("Server=localhost;Database=unused;Integrated Security=true;");
        builder.UseLowerCaseNamingConvention();
        using var context = new WriteDbContext(builder.Options,new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {["DatabaseProvider"]=provider}).Build());
        var customer=context.Model.FindEntityType(typeof(Customer));Assert.NotNull(customer);
        Assert.Equal("customer",customer.GetTableName());
        Assert.True(customer.FindProperty(nameof(Customer.Revision))?.IsConcurrencyToken);
        Assert.Contains(customer.GetIndexes(),index=>index.IsUnique&&index.Properties.Single().Name==nameof(Customer.SourceLeadId));
        var contact=context.Model.FindEntityType(typeof(CustomerContact));Assert.NotNull(contact);
        Assert.False(contact.IsOwned());Assert.Contains(contact.GetForeignKeys(),key=>key.PrincipalEntityType.ClrType==typeof(Customer));
        var sql=context.Set<Customer>().Where(item=>item.DeletedAt==null&&item.Business.Contains("Aurora")).OrderByDescending(item=>item.CreatedAt).ThenByDescending(item=>item.Id).Skip(20).Take(20).ToQueryString();
        Assert.Contains("OFFSET",sql);Assert.Contains("deletedat",sql);Assert.Contains("customer",sql);
    }
}
