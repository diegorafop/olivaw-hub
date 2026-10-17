using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mps.Limpo.Dados.Repositories;
using Mps.HolosCore.Repositories;
using Mps.HolosCore.Dados.Extensions;

namespace Mps.Limpo.Dados.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRepositories
    (
        this IServiceCollection services
    )
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }

    public static void AddDbContext
    (
        this IServiceCollection services,
        ConfigurationManager configuration,
        bool isProduction
    )
    {
        const string migrationsTableName = "__efmigrationshistory";

        var connectionString = configuration.GetConnectionString("DbContext");
        var databaseProvider = configuration["DatabaseProvider"];

        var connectionStringDictionary = configuration.GetConnectionStringToDictionary();

        var schema = connectionStringDictionary["searchpath"];
        var migrationsProjectAssemblyName = $"Mps.Limpo.Dados.{databaseProvider}";

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
                    ),

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

                "Oracle" => options
                    .UseOracle
                    (
                        connectionString,
                        item =>
                        {
                            item.CommandTimeout(30);
                            item.MigrationsAssembly(migrationsProjectAssemblyName);
                        }
                    )
                    .UseUpperCaseNamingConvention(),

                _ => throw new Exception($"Database provider não suportado: {databaseProvider}")
            };
        });
    }

}
