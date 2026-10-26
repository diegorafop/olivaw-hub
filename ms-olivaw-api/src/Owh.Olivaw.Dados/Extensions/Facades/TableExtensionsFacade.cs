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
        modelBuilder.SetupCustomer();
        modelBuilder.SetupManagedService();
        modelBuilder.SetupInventory();
        modelBuilder.SetupPlans();
        modelBuilder.SetupExpenses();
        modelBuilder.SetupPortal();
        modelBuilder.SetupPortalUsage();
        modelBuilder.SetupDeliveryNotes();
        modelBuilder.SetupCareEvidence();
        modelBuilder.SetupUsageReading();
        modelBuilder.SetupHealthReading();
        modelBuilder.SetupSupportRequest();
        modelBuilder.SetupIncident();
        modelBuilder.SetupSecurityReport();
        modelBuilder.SetupSecurityRemediation();
    }
}
