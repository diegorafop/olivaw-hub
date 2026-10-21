using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Dados.Extensions.Tables;

namespace Owh.Olivaw.Dados.Extensions.Facades;

internal static class TableExtensionsFacade
{
    internal static void SetupTables
    (
        this ModelBuilder modelBuilder
    )
    {
        modelBuilder.SetupLead();
    }
}