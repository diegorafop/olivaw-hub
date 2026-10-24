using Microsoft.EntityFrameworkCore;
using Owh.Ltmj.Dados.Extensions.Tables;

namespace Owh.Ltmj.Dados.Extensions.Facades;

internal static class TableExtensionsFacade
{
    internal static void SetupTables
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder.SetupPortfolio();
    }
}