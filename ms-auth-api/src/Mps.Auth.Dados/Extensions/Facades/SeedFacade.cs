using Microsoft.EntityFrameworkCore.Migrations;
using Mps.Auth.Dados.Extensions.Tables;

namespace Mps.Auth.Dados.Extensions.Facades;

public static class SeedFacade
{
    public static void DbCreateSeed
    (
        this MigrationBuilder migrationBuilder
    )
    {
        migrationBuilder.SeedUsuarioRootDbCreateData();
        migrationBuilder.SeedAutorizacaoDbCreateDataFacade();
        migrationBuilder.SedUsuarioAutorizacaoDbCreateData();

        migrationBuilder.SeedPerfilAcessoPerfilBasico();
        migrationBuilder.SeedAutorizacaoPerfilAcessoAutorizacaoPerfilBasico();
    }
}