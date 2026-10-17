using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Mps.Auth.Domain;

namespace Mps.Auth.Dados.Extensions.Tables;

public static class PerfilAcessoExtensions
{
    internal static void SetupPerfilAcesso
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder
            .Entity<PerfilAcesso>()
            .Property(item => item.Nome)
            .HasMaxLength(100);

        modelBuilder
            .Entity<PerfilAcesso>()
            .Property(item => item.Descricao)
            .HasMaxLength(200);

        modelBuilder
            .Entity<PerfilAcesso>()
            .HasIndex(item => item.IdExterno);

        modelBuilder
            .Entity<PerfilAcesso>()
            .HasMany(item => item.Autorizacaos);
    }

    public static void SeedPerfilAcessoPerfilBasico
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataPerfilAcessoPerfilBasico(migrationBuilder);
    }

    private static void InsertDataPerfilAcessoPerfilBasico
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "perfilacesso",
            new string[]
            {
                "idexterno",
                "usuariologadoid",
                "datacadastro",
                "dataatualizacao",
                "dataremocao",
                "nome",
                "descricao"
            },
            new object[]
            {
                Guid.NewGuid(),
                Guid.Empty,
                DateTime.UtcNow,
                null,
                null,
                "Perfil básico",
                "Perfil básico"
            }
        );
    }
}