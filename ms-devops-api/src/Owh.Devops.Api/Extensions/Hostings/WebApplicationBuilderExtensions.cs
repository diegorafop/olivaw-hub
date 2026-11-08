using Microsoft.EntityFrameworkCore;
using Owh.Devops.Dados;
using Owh.Devops.Dados.Extensions.Facades;
using System.Diagnostics.CodeAnalysis;

namespace Owh.Devops.Api.Extensions.Hostings;

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
    }
}
