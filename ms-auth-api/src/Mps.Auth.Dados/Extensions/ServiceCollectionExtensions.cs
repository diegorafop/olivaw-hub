using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mps.Auth.Dados.Repositories;
using Mps.Auth.Repositories;
using Mps.HolosCore.Dados.Extensions;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Dados.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRepositories
    (
        this IServiceCollection services
    )
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IRepositoryUsuario, RepositoryUsuario>();
        services.AddScoped<IRepositoryPerfilAcesso, RepositoryPerfilAcesso>();
    }

    public static void AddDbContext
    (
        this IServiceCollection services,
        IConfiguration configuration,
        bool isProduction
    )
    {
        const string migrationsTableName = "__efmigrationshistory";

        var connectionString = configuration.GetConnectionString("DbContext");
        var connectionStringDictionary = configuration.GetConnectionStringToDictionary();

        var databaseProvider = configuration["DatabaseProvider"];
        var schema = connectionStringDictionary["searchpath"];
        var migrationsProjectAssemblyName = $"Mps.Auth.Dados.{databaseProvider}";

        services.AddDbContext<WriteDbContext>(options =>
        {
            if (!isProduction)
                options.EnableSensitiveDataLogging();

            _ = databaseProvider switch
            {
                "PostgreSql" => options
                    .UseLowerCaseNamingConvention()
                    .UseNpgsql
                    (
                        connectionString,
                        item =>
                        {
                            item.MigrationsAssembly(migrationsProjectAssemblyName);
                            item.MigrationsHistoryTable
                            (
                                migrationsTableName,
                                schema
                            );
                        }
                    )
                    .ConfigureWarnings(),

                "SqlServer" => options
                .UseLowerCaseNamingConvention()
                .UseSqlServer
                (
                    connectionString,
                    item =>
                    {
                        item.MigrationsAssembly(migrationsProjectAssemblyName);
                        item.MigrationsHistoryTable
                        (
                            migrationsTableName,
                            schema
                        );
                    }
                ),

                _ => throw new Exception($"Database provider não suportado: {databaseProvider}")
            };
        });
    }
}