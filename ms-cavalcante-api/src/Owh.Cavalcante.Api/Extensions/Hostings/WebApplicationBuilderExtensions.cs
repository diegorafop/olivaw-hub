using Microsoft.EntityFrameworkCore;
using Olivaw.Identity.Api.Services;
using Owh.Cavalcante.Dados;
using Owh.Cavalcante.Dados.Extensions.Facades;
using System.Diagnostics.CodeAnalysis;

namespace Owh.Cavalcante.Api.Extensions.Hostings;

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
        await scope.ServiceProvider.GetRequiredService<RootBootstrap>()
            .EnsureAsync(webApplication.Configuration, cancellationToken);
    }
}
