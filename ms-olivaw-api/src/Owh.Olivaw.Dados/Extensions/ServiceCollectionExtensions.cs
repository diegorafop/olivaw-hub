using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Owh.Olivaw.Dados.Repositories;
using Owh.OlivawCore.Repositories;
using Owh.OlivawCore.Dados.Extensions;

namespace Owh.Olivaw.Dados.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRepositories
    (
        this IServiceCollection services
    )
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<Owh.Olivaw.Repositories.IRepositoryLead, RepositoryLead>();
        services.AddScoped<Owh.Olivaw.Repositories.IRepositoryCustomer, RepositoryCustomer>();
        services.AddScoped<Owh.Olivaw.Repositories.IRepositoryManagedService, RepositoryManagedService>();
        services.AddScoped<Owh.Olivaw.Repositories.IRepositoryInventoryRevision, RepositoryInventoryRevision>();
        services.AddScoped<Owh.Olivaw.Repositories.IRepositoryInventoryResource, RepositoryInventoryResource>();
        services.AddScoped<Owh.Olivaw.Repositories.IRepositoryServiceDeployment, RepositoryServiceDeployment>();
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

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Configure ConnectionStrings:DbContext para o cliente.");

        var schema = configuration["Database:Schema"];
        if (databaseProvider == "PostgreSql")
        {
            var connection = new NpgsqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(schema)) schema = connection.SearchPath;
        }
        var migrationsProjectAssemblyName = $"Owh.Olivaw.Dados.{databaseProvider}";

        services.AddDbContext<WriteDbContext>(options =>
        {

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
