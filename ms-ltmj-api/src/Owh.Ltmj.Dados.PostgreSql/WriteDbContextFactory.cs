using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Owh.Ltmj.Dados.PostgreSql;

/// <summary>Contexto de design time: gerar migrations não inicializa serviços HTTP ou bootstrap.</summary>
public sealed class WriteDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
{
    public WriteDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseProvider"] = "PostgreSql"
        }).Build();
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseLowerCaseNamingConvention()
            .UseNpgsql("Host=localhost;Database=ltmj_design;SearchPath=ms_ltmj;Username=postgres;", provider =>
            {
                provider.MigrationsAssembly(typeof(WriteDbContextFactory).Assembly.GetName().Name);
                provider.MigrationsHistoryTable("__efmigrationshistory", "ms_ltmj");
            });
        return new WriteDbContext(options.Options, configuration);
    }
}
