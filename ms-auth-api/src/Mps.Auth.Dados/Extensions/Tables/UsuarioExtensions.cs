using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Mps.Auth.Dados.Extensions.Helpers;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;

namespace Mps.Auth.Dados.Extensions.Tables;

internal static class UsuarioExtensions
{
    internal static void SetupUsuario
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder
            .Entity<Usuario>()
            .Property(item => item.Login)
            .HasMaxLength(20);

        modelBuilder
            .Entity<Usuario>()
            .Property(item => item.Senha)
            .HasMaxLength(70);

        modelBuilder
            .Entity<Usuario>()
            .Property(item => item.Nome)
            .HasMaxLength(60);

        modelBuilder
            .Entity<Usuario>()
            .Property(item => item.Email)
            .HasMaxLength(40);

        modelBuilder
            .Entity<Usuario>()
            .HasMany(item => item.Autorizacoes);

        modelBuilder
            .Entity<Usuario>()
            .HasMany(item => item.Perfils);
    }

    internal const string PasswordDefault = "mps2026#";

    internal static void SeedUsuarioRootDbCreateData
    (
        this MigrationBuilder migrationBuilder
    )
    {
        var configuration = EnvironmentHelper.GetConfiguration();
        var senha = GetSenhaRoot(configuration);

        migrationBuilder.InsertData
        (
            "usuario",
            [
                "idexterno",
                "usuariologadoid",
                "datacadastro",
                "dataatualizacao",
                "dataremocao",
                "login",
                "senha",
                "nome",
                "email",
                "integracaoid",
                "statusintegracaoenum"
            ],
            [
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "root",
                BCrypt.Net.BCrypt.EnhancedHashPassword(senha),
                "Hólos",
                "holos@mps.com.br",
                null,
                (int)StatusIntegracaoEnum.NaoIntegrar
            ]
        );
    }

    private static string GetSenhaRoot
    (
        Microsoft.Extensions.Configuration.IConfigurationRoot configuration
    )
    {
        var section = configuration.GetSection
        (
            "services:provider-authentication:senha"
        );

        if (section is null)
            return PasswordDefault;

        if (string.IsNullOrEmpty(section.Value))
            return PasswordDefault;

        return section
            .Value
            .ToString();
    }
}