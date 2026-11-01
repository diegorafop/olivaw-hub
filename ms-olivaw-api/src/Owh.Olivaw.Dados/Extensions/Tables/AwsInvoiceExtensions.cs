using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Dados.Extensions.Tables;
internal static class AwsInvoiceExtensions
{
    internal static void SetupAwsInvoices(this ModelBuilder builder)
    {
        var e=builder.Entity<AwsInvoice>(); e.ToTable("awsinvoice"); e.HasKey(x=>x.Id);
        e.HasIndex(x=>x.ExternalId).IsUnique(); e.HasIndex(x=>x.SubmissionKey).IsUnique(); e.HasIndex(x=>x.Digest).IsUnique(); e.HasIndex(x=>new{x.BillingYear,x.BillingMonth});
        e.Property(x=>x.Digest).HasMaxLength(64).IsRequired(); e.Property(x=>x.Issuer).HasMaxLength(200).IsRequired(); e.Property(x=>x.AccountReference).HasMaxLength(120).IsRequired(); e.Property(x=>x.InvoiceNumber).HasMaxLength(120);
        e.Property(x=>x.Currency).HasMaxLength(3).IsRequired(); e.Property(x=>x.DocumentStatus).HasMaxLength(20).IsRequired(); e.Property(x=>x.Completeness).HasMaxLength(20).IsRequired(); e.Property(x=>x.ExchangeRateNature).HasMaxLength(20).IsRequired();
        e.Property(x=>x.Source).HasMaxLength(300).IsRequired(); e.Property(x=>x.RawData).IsRequired();
        e.Property(x=>x.GrossAmount).HasPrecision(24,10); e.Property(x=>x.CreditAmount).HasPrecision(24,10); e.Property(x=>x.TaxAmount).HasPrecision(24,10); e.Property(x=>x.NetAmount).HasPrecision(24,10); e.Property(x=>x.ExchangeRateToBrl).HasPrecision(18,6);
        var link=builder.Entity<AwsInvoiceExpense>();link.ToTable("awsinvoiceexpense");link.HasKey(x=>x.Id);link.HasIndex(x=>x.ExpenseId).IsUnique();link.HasIndex(x=>x.ExternalId).IsUnique();
        link.HasOne(x=>x.AwsInvoice).WithMany(x=>x.Expenses).HasForeignKey(x=>x.AwsInvoiceId).OnDelete(DeleteBehavior.Restrict);
        link.HasOne(x=>x.Expense).WithMany().HasForeignKey(x=>x.ExpenseId).OnDelete(DeleteBehavior.Restrict);
    }
}
