using Microsoft.EntityFrameworkCore;
using Mps.Limpo.Dados.Extensions.Tables;

namespace Mps.Limpo.Dados.Extensions.Facades;

internal static class TableExtensionsFacade
{
    internal static void SetupTables
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder.SetupExample();
    }
}