using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Mps.Auth.Domain;

namespace Mps.Auth.Dados.Extensions.Tables;

internal static class UsuarioAutorizacaoExtensions
{
    internal static void SetupUsuarioAutorizacao
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder
            .Entity<UsuarioAutorizacao>()
            .HasOne(item => item.Usuario);

        modelBuilder
            .Entity<UsuarioAutorizacao>()
            .HasOne(item => item.Autorizacao);
    }

    internal static void SedUsuarioAutorizacaoDbCreateData
    (
        this MigrationBuilder migrationBuilder
    )
    {
        Guid usuarioId = Guid.NewGuid();
        migrationBuilder.InsertData
        (
            "usuarioautorizacao",
            new string[]
            {
                "idexterno",
                "usuariologadoid",
                "datacadastro",
                "dataatualizacao",
                "dataremocao",
                "usuarioid",
                "autorizacaoid"
            },
            new object[]
            {
                usuarioId,
                usuarioId,
                DateTime.UtcNow,
                null,
                null,
                "(select max(id) from usuario where login='root')",
                "(select max(id) from autorizacao where valor='root')"
            }
        );
    }
}