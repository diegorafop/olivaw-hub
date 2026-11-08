using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class ExpenseExtensions
{
    internal static void SetupExpenses(this ModelBuilder builder)
    {
        var e = builder.Entity<Expense>(); e.ToTable("expense"); e.HasKey(x => x.Id);
        e.HasIndex(x => x.ExternalId).IsUnique(); e.HasIndex(x => x.EntryKey).IsUnique(); e.HasIndex(x => new { x.OccurredAt, x.CustomerId });
        e.Property(x => x.Digest).HasMaxLength(64).IsRequired(); e.Property(x => x.Description).HasMaxLength(160).IsRequired();
        e.Property(x => x.Category).HasMaxLength(30).IsRequired(); e.Property(x => x.Kind).HasMaxLength(10).IsRequired(); e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        e.Property(x => x.Source).HasMaxLength(300).IsRequired(); e.Property(x => x.Amount).HasPrecision(18, 2); e.Property(x => x.ExchangeRateToBrl).HasPrecision(18, 6); e.Property(x => x.AmountBrl).HasPrecision(18, 2);
        e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
