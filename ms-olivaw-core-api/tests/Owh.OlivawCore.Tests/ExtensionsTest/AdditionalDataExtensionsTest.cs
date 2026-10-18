using Microsoft.Extensions.Configuration;
using Owh.OlivawCore.Dados.Extensions;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class AdditionalDataExtensionsTest
{
    [Theory]
    [InlineData("PostgreSql", null)]
    [InlineData("SqlServer", "")]
    [InlineData("Oracle", "")]
    [InlineData("Oracle", "User Id=sample")]
    public void GetDbContextHost_should_not_invent_a_host_for_incomplete_configuration(string provider, string? connection)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = provider,
            ["ConnectionStrings:DbContext"] = connection
        }).Build();
        Assert.Throws<KeyNotFoundException>(() => configuration.GetDbContextHost());
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    public void ConstraintBuilder_should_truncate_long_fields_and_keep_the_id_suffix(int fieldLength)
    {
        var builder = new ConstraintBuilder
        {
            TableNamePrimary = new string('P', 50),
            TableNameSecondary = new string('S', 50),
            FieldName = new string('F', fieldLength) + "Id",
            MaximumNameLength = 63,
            PostfixName = "_fk",
            IdKey = "id"
        };
        builder.Build();
        Assert.InRange(builder.GetName().Length, 1, 63);
        Assert.EndsWith("id_fk", builder.GetName());
        Assert.StartsWith("PPP", builder.GetName());
    }

    [Fact]
    public void ConstraintBuilder_should_reallocate_available_space_when_secondary_name_becomes_shorter()
    {
        var builder = new ConstraintBuilder
        {
            TableNamePrimary = new string('P', 50),
            TableNameSecondary = new string('S', 50),
            FieldName = "ParentId",
            MaximumNameLength = 63,
            PostfixName = "_fk",
            IdKey = "id"
        };
        builder.Build();
        builder.TableNamePrimary = new string('P', 50);
        builder.TableNameSecondary = "Child";
        builder.Build();
        Assert.InRange(builder.GetName().Length, 1, 63);
        Assert.Contains("_Child_", builder.GetName());
        Assert.EndsWith("Parentid_fk", builder.GetName());
    }
}
