using Microsoft.EntityFrameworkCore;
using Mps.Limpo.Dados;
using Mps.Limpo.Dados.Extensions.Facades;
using System.Diagnostics.CodeAnalysis;

namespace Mps.Limpo.Api.Extensions.Hostings;

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

    public static void UseMigrations(this WebApplication webApplication)
    {
        using var scope = webApplication.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        database.Database.Migrate();
    }
}
