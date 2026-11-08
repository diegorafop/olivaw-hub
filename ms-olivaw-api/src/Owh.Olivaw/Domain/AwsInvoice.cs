using System;
using System.Collections.Generic;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class AwsInvoice : Entity
{
    public Guid SubmissionKey { get; private set; }
    public string Digest { get; private set; } = string.Empty;
    public string Issuer { get; private set; } = string.Empty;
    public string AccountReference { get; private set; } = string.Empty;
    public string? InvoiceNumber { get; private set; }
    public int BillingYear { get; private set; }
    public int BillingMonth { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string DocumentStatus { get; private set; } = string.Empty;
    public string Completeness { get; private set; } = string.Empty;
    public decimal? GrossAmount { get; private set; }
    public decimal? CreditAmount { get; private set; }
    public decimal? TaxAmount { get; private set; }
    public decimal? NetAmount { get; private set; }
    public decimal? ExchangeRateToBrl { get; private set; }
    public string ExchangeRateNature { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string RawData { get; private set; } = string.Empty;
    public ICollection<AwsInvoiceExpense> Expenses { get; private set; } = new List<AwsInvoiceExpense>();
    private AwsInvoice() { }
    public static AwsInvoice Create(Guid key, string digest, string issuer, string account, string? number, int year, int month, string currency, string status, string completeness, decimal? gross, decimal? credit, decimal? tax, decimal? net, decimal? fx, string fxNature, string source, string rawData, Guid actor, DateTime now)
    {
        if (key == Guid.Empty || actor == Guid.Empty || digest.Length != 64) throw new ArgumentException("Identificação inválida.");
        if (string.IsNullOrWhiteSpace(issuer) || issuer.Length > 200 || string.IsNullOrWhiteSpace(account) || account.Length > 120 || number?.Length > 120 || string.IsNullOrWhiteSpace(source) || source.Length > 300) throw new ArgumentException("Identificação documental inválida.");
        if (year < 2000 || year > 2100 || month < 1 || month > 12 || currency is not ("BRL" or "USD")) throw new ArgumentException("Competência ou moeda inválida.");
        if (status is not ("provisional" or "issued" or "paid") || completeness is not ("complete" or "issuer-section" or "partial")) throw new ArgumentException("Situação documental inválida.");
        if (status == "paid" && (completeness != "complete" || string.IsNullOrWhiteSpace(number))) throw new ArgumentException("Pagamento exige documento completo identificado.");
        foreach (var value in new[] { gross, credit, tax, net }) if (value < 0 || value > 1000000000 || (value.HasValue && decimal.Round(value.Value, 10) != value)) throw new ArgumentException("Total inválido.");
        if (gross.HasValue && credit.HasValue && tax.HasValue && net.HasValue && Math.Abs(gross.Value - credit.Value + tax.Value - net.Value) > 0.01m) throw new ArgumentException("Totais não reconciliados.");
        if (fx <= 0 || fx > 1000 || (fx.HasValue && decimal.Round(fx.Value,6) != fx) || (currency == "BRL" && fx != 1) || fxNature is not ("unknown" or "managerial" or "payment") || (!fx.HasValue && fxNature != "unknown")) throw new ArgumentException("Conversão inválida.");
        if (string.IsNullOrWhiteSpace(rawData) || rawData.Length > 1000000) throw new ArgumentException("Dados originais obrigatórios.");
        return new() { ExternalId=Guid.NewGuid(), SubmissionKey=key, Digest=digest, Issuer=issuer, AccountReference=account, InvoiceNumber=number, BillingYear=year, BillingMonth=month, Currency=currency, DocumentStatus=status, Completeness=completeness, GrossAmount=gross, CreditAmount=credit, TaxAmount=tax, NetAmount=net, ExchangeRateToBrl=fx, ExchangeRateNature=fxNature, Source=source, RawData=rawData, ActorUserId=actor, CreatedAt=now };
    }
    public void Link(Expense expense, Guid actor, DateTime now)
    {
        if (expense == null || actor == Guid.Empty) throw new ArgumentException("Vínculo inválido.");
        Expenses.Add(AwsInvoiceExpense.Create(this,expense,actor,now));
    }
}
public sealed class AwsInvoiceExpense : Entity
{
    public int AwsInvoiceId { get; private set; }
    public AwsInvoice AwsInvoice { get; private set; } = null!;
    public int ExpenseId { get; private set; }
    public Expense Expense { get; private set; } = null!;
    private AwsInvoiceExpense() { }
    internal static AwsInvoiceExpense Create(AwsInvoice invoice, Expense expense, Guid actor, DateTime now) => new() { ExternalId=Guid.NewGuid(), AwsInvoice=invoice, Expense=expense, ExpenseId=expense.Id, ActorUserId=actor, CreatedAt=now };
}
