using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Owh.Olivaw.Dados.PostgreSql;

/// <summary>Geração offline do schema da autoridade do portal, sem host ou bootstrap.</summary>
public sealed class PortalIdentityDbContextFactory : IDesignTimeDbContextFactory<PortalIdentityDbContext>
{
    public PortalIdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PortalIdentityDbContext>()
            .UseLowerCaseNamingConvention()
            .UseNpgsql("Host=localhost;Database=olivaw_design;SearchPath=ms_olivaw;Username=postgres;", provider =>
            {
                provider.MigrationsAssembly(typeof(PortalIdentityDbContextFactory).Assembly.GetName().Name);
                provider.MigrationsHistoryTable("portalidentitymigrations", "ms_olivaw");
            });
        return new PortalIdentityDbContext(options.Options);
    }
}
