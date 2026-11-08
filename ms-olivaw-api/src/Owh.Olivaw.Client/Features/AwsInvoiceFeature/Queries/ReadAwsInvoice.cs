using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.AwsInvoiceFeature.Queries;
public sealed class ReadAwsInvoiceByFilterQuery { public int? BillingYear { get; set; } public int? BillingMonth { get; set; } public int BeforeId { get; set; } }
public sealed class ReadAwsInvoiceByFilterResponse { public List<AwsInvoiceItem> Items { get; set; } = new(); public int? NextBeforeId { get; set; } }
public sealed class ReadAwsInvoiceByIdQuery { public Guid ExternalId { get; set; } }
public sealed class ReadAwsInvoiceByIdResponse { public AwsInvoiceItem Invoice { get; set; } = new(); public string RawData { get; set; } = string.Empty; public List<AwsInvoiceExpenseItem> Expenses { get; set; } = new(); }
public sealed class AwsInvoiceExpenseItem { public Guid ExternalId { get; set; } public string Description { get; set; } = string.Empty; public decimal AmountBrl { get; set; } public string Kind { get; set; } = string.Empty; }
public sealed class AwsInvoiceItem
{
    public Guid ExternalId { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string AccountReference { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public int BillingYear { get; set; }
    public int BillingMonth { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string DocumentStatus { get; set; } = string.Empty;
    public string Completeness { get; set; } = string.Empty;
    public decimal? GrossAmount { get; set; }
    public decimal? CreditAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? ExchangeRateToBrl { get; set; }
    public string ExchangeRateNature { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
