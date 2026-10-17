using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Mps.Auth.Domain;

namespace Mps.Auth.Dados.Extensions.Tables;

public static class PerfilAcessoAutorizacaoExtensions
{
    public static void SetupPerfilAcessoAutorizacao
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder
            .Entity<PerfilAcessoAutorizacao>()
            .HasIndex(item => item.IdExterno);

        modelBuilder
            .Entity<PerfilAcessoAutorizacao>()
            .HasOne(item => item.PerfilAcesso);

        modelBuilder
            .Entity<PerfilAcessoAutorizacao>()
            .HasOne(item => item.Autorizacao);
    }

    public static void SeedAutorizacaoPerfilAcessoAutorizacaoPerfilBasico
    (
        this MigrationBuilder migrationBuilder
    )
    {
        InsertDataAutorizacaoPerfilAcessoPerfilBasico(migrationBuilder);
    }

    private static void InsertDataAutorizacaoPerfilAcessoPerfilBasico
    (
        MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.InsertData
        (
            "perfilacessoautorizacao",
            new string[]
            {
                "idexterno",
                "usuariologadoid",
                "datacadastro",
                "dataatualizacao",
                "dataremocao",
                "autorizacaoid",
                "perfilacessoid"
            },
            new object[]
            {
                Guid.NewGuid(), Guid.Empty, DateTime.UtcNow, null, null,
                $"(select max(id) from autorizacao where tipo='Role' and valor='Dashboard')",
                $"(select max(id) from perfilacesso where nome='Perfil básico')"
            }
        );
    }
}