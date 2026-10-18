using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Configuration;
using Owh.OlivawCore.Dados.Extensions;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class DataExtensionsTest
{
    [Theory]
    [InlineData("PostgreSql", "Host=database.test;Database=sample", "database.test")]
    [InlineData("SqlServer", "Host=database.test;Database=sample", "database.test")]
    [InlineData("Oracle", "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.test)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=sample)));User Id=test", "oracle.test")]
    public void GetDbContextHost_should_parse_supported_connection_formats(string provider, string connection, string expected)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = provider,
            ["ConnectionStrings:DbContext"] = connection
        }).Build();
        Assert.Equal(expected, config.GetDbContextHost());
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData(null)]
    public void GetDbContextHost_should_reject_unsupported_provider(string? provider)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = provider
        }).Build();
        var exception = Assert.Throws<Exception>(() => config.GetDbContextHost());
        Assert.Contains("não suportado", exception.Message);
    }

    [Theory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    [InlineData("Oracle")]
    public void OnConfiguring_should_apply_provider_conventions_without_connecting(string provider)
    {
        var options = new DbContextOptionsBuilder();
        options.OnConfiguring(provider);
        Assert.Equal(provider != "SqlServer", options.Options.Extensions.Any());
    }

    [Theory]
    [InlineData(null, "não encontrado")]
    [InlineData("", "não encontrado")]
    [InlineData("Unknown", "não suportado")]
    public void OnConfiguring_should_reject_missing_or_unknown_provider(string? provider, string message)
    {
        var exception = Assert.Throws<Exception>(() => new DbContextOptionsBuilder().OnConfiguring(provider!));
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void SetupConstraints_should_name_primary_foreign_and_index_keys()
    {
        var model = BuildModel("Parent", "Child", "ParentId");
        model.SetupConstraints();
        var child = model.Model.FindEntityType("Child")!;
        Assert.Equal("child_pk", child.FindPrimaryKey()!.GetName());
        Assert.Equal("parent_child_parentid_fk", Assert.Single(child.GetForeignKeys()).GetConstraintName());
        Assert.Equal("child_parentid_ix", Assert.Single(child.GetIndexes()).GetDatabaseName());
    }

    [Theory]
    [InlineData(58)]
    [InlineData(59)]
    [InlineData(60)]
    [InlineData(80)]
    public void SetupConstraints_should_handle_table_names_at_the_length_limit(int length)
    {
        var model = BuildModel(new string('P', length), new string('C', length), "ParentId");
        model.SetupConstraints();
        foreach (var entity in model.Model.GetEntityTypes())
        {
            Assert.InRange(entity.FindPrimaryKey()!.GetName()!.Length, 1, 63);
            Assert.EndsWith("_pk", entity.FindPrimaryKey()!.GetName());
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                Assert.InRange(foreignKey.GetConstraintName()!.Length, 1, 63);
                Assert.EndsWith("_fk", foreignKey.GetConstraintName());
            }
            foreach (var index in entity.GetIndexes())
                Assert.InRange(index.GetDatabaseName()!.Length, 1, 63);
        }
    }

    [Fact]
    public void SetupConstraints_should_handle_single_character_index_property()
    {
        var model = BuildModel("Parent", new string('C', 80), "X");
        model.SetupConstraints();
        Assert.EndsWith("_x_ix", Assert.Single(model.Model.FindEntityType("Child")!.GetIndexes()).GetDatabaseName());
    }

    private static ModelBuilder BuildModel(string parentTable, string childTable, string foreignKey)
    {
        var model = new ModelBuilder(new ConventionSet());
        var parent = model.Entity("Parent");
        parent.Property<int>("Id");
        parent.HasKey("Id");
        parent.ToTable(parentTable);
        var child = model.Entity("Child");
        child.Property<int>("Id");
        child.Property<int>(foreignKey);
        child.HasKey("Id");
        child.ToTable(childTable);
        child.HasOne("Parent").WithMany().HasForeignKey(foreignKey);
        child.HasIndex(foreignKey);
        return model;
    }
}
