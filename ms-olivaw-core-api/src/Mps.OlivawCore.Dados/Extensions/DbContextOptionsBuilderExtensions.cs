using Microsoft.EntityFrameworkCore;

namespace Mps.OlivawCore.Dados.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static void OnConfiguring
    (
        this DbContextOptionsBuilder optionsBuilder,
        string databaseProvider
    )
    {
        if (string.IsNullOrEmpty(databaseProvider))
            throw new Exception($"Database provider não encontrado");

        _ = databaseProvider switch
        {
            "PostgreSql" => optionsBuilder.UseLowerCaseNamingConvention(),
            "SqlServer" => optionsBuilder,
            "Oracle" => optionsBuilder.UseUpperCaseNamingConvention(),

            _ => throw new Exception($"Database provider não suportado: {databaseProvider}")
        };
    }
}