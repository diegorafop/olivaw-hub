using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Owh.OlivawCore.Dados.Extensions;

public static class ServiceCollectionExtensions
{
    public static DbContextOptionsBuilder ConfigureWarnings
    (
        this DbContextOptionsBuilder optionsBuilder
    )
    {
        return optionsBuilder.ConfigureWarnings
        (
            itens => itens.Ignore
            (
                RelationalEventId.CommandCanceled,
                RelationalEventId.CommandCreated,
                RelationalEventId.CommandCreating,
                RelationalEventId.CommandInitialized,
                RelationalEventId.CommandExecuting,
                RelationalEventId.DataReaderClosing,
                RelationalEventId.DataReaderDisposing,
                RelationalEventId.MigrationApplying,
                RelationalEventId.AcquiringMigrationLock,
                RelationalEventId.MigrateUsingConnection
            )
        );
    }

    public static string GetDbContextHost
    (
        this IConfiguration configuration
    )
    {
        var itens = GetConnectionStringToDictionary(configuration);

        return itens["host"];
    }

    public static Dictionary<string, string> GetConnectionStringToDictionary
    (
        this IConfiguration configuration
    )
    {
        var databaseProvider = configuration["DatabaseProvider"];
        string connectionString = configuration.GetConnectionString("DbContext");

        var itens = databaseProvider switch
        {
            "PostgreSql" => PostgreSqlConnectionStringToDictionary(connectionString),

            "SqlServer" => SqlServerConnectionStringToDictionary(connectionString),

            "Oracle" => OracleConnectionStringToDictionary(connectionString),

            _ => throw new Exception($"Database provider não suportado: {databaseProvider}")
        };

        return itens.ToDictionary
        (
            pair => pair
                .Key
                .ToLowerInvariant(),
            pair => pair.Value
        );
    }

    private static Dictionary<string, string> SqlServerConnectionStringToDictionary
    (
        string connectionString
    )
    {
        if (string.IsNullOrEmpty(connectionString))
            return [];

        return connectionString
            .Split
            (
                [";"],
                StringSplitOptions.RemoveEmptyEntries
            )
            .Select(item => item.Split('='))
            .ToDictionary(item => item[0], item => item[1]);
    }

    private static Dictionary<string, string> PostgreSqlConnectionStringToDictionary
    (
        string connectionString
    )
    {
        if (string.IsNullOrEmpty(connectionString))
            return [];

        return connectionString
            .Split
            (
                [";"],
                StringSplitOptions.RemoveEmptyEntries
            )
            .Select(item => item.Split('='))
            .ToDictionary(item => item[0], item => item[1]);
    }

    private static Dictionary<string, string> OracleConnectionStringToDictionary
    (
        string connectionString
    )
    {
        if (string.IsNullOrEmpty(connectionString))
            return [];

        var itens = connectionString
            .Split
            (
                [";"],
                StringSplitOptions.RemoveEmptyEntries
            );

        var response = itens
            .Where(item => !item.Contains("data source", StringComparison.CurrentCultureIgnoreCase))
            .Select(item => item.Split('='))
            .ToDictionary(item => item[0], item => item[1]);

        response = response
            .Concat(GetManyByDataSource(itens))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        response.TryAdd("SearchPath", string.Empty);

        return response;
    }

    private static Dictionary<string, string> GetManyByDataSource
    (
        string[] itens
    )
    {
        var dataSource = itens.FirstOrDefault(SearchPredicateByName("data source"));

        if (dataSource is null)
            return [];

        return dataSource
            .Replace("=(", "___")
            .Replace("(", "___")
            .Replace(")", "___")
            .Split(["___"], StringSplitOptions.RemoveEmptyEntries)
            .Where(item => item.Contains('='))
            .Select(item => item.Split('='))
            .ToDictionary(item => item[0], item => item[1]);
    }

    private static Func<string, bool> SearchPredicateByName
    (
        string name
    )
    {
        return item => item.Contains
        (
            name,
            StringComparison.CurrentCultureIgnoreCase
        );
    }
}