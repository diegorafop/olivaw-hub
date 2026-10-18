using Microsoft.EntityFrameworkCore;
using Mps.Ltmj.Dados.Extensions.Tables;

namespace Mps.Ltmj.Dados.Extensions.Facades;

internal static class TableExtensionsFacade
{
    internal static void SetupTables
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder.SetupExample();
        modelBuilder.SetupPortfolio();
    }
}