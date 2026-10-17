using Microsoft.EntityFrameworkCore;
using Mps.Auth.Dados.Extensions.Tables;

namespace Mps.Auth.Dados.Extensions.Facades;

internal static class TableExtensionsFacade
{
    internal static void SetupTables
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder.SetupUsuario();
        modelBuilder.SetupAutorizacao();
        modelBuilder.SetupUsuarioAutorizacao();
        modelBuilder.SetupUsuarioAutenticacao();
        modelBuilder.SetupPerfilAcesso();
        modelBuilder.SetupPerfilAcessoAutorizacao();
        modelBuilder.SetupUsuarioPerfilAcesso();
    }
}