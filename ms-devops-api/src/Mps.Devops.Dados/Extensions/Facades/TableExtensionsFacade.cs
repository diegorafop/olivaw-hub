using Microsoft.EntityFrameworkCore;
using Mps.Devops.Dados.Extensions.Tables;

namespace Mps.Devops.Dados.Extensions.Facades;

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