using Owh.Olivaw.Api.Features.PortalFeature.Internals;
using Microsoft.EntityFrameworkCore;
using Olivaw.Identity.Api.Services;
using Owh.Olivaw.Dados;
using Owh.Olivaw.Dados.Extensions.Facades;
using System.Diagnostics.CodeAnalysis;

namespace Owh.Olivaw.Api.Extensions.Hostings;

[ExcludeFromCodeCoverage]
public static class WebApplicationBuilderExtensions
{
    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDatabase
        (
            builder.Configuration,
            builder.Environment.IsProduction()
        );
    }

    public static async Task InitializeDatabaseAsync(this WebApplication webApplication, CancellationToken cancellationToken)
    {
        await using var scope = webApplication.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        await database.Database.MigrateAsync(cancellationToken);
        var portal = scope.ServiceProvider.GetService<PortalIdentityDbContext>();
        if (portal != null)
        {
            if (portal.Database.HasPendingModelChanges()) throw new InvalidOperationException("A identidade do portal possui modelo pendente de migration própria.");
            await portal.Database.MigrateAsync(cancellationToken);
        }
        await scope.ServiceProvider.GetRequiredService<RootBootstrap>()
            .EnsureAsync(webApplication.Configuration, cancellationToken);
    }
}
