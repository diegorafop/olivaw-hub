using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.AwsInvoiceFeature.Commands;
public sealed class CreateAwsInvoiceCommand
{
    public Guid SubmissionKey { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string AccountReference { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public int BillingYear { get; set; }
    public int BillingMonth { get; set; }
    public string Currency { get; set; } = "USD";
    public string DocumentStatus { get; set; } = "provisional";
    public string Completeness { get; set; } = "partial";
    public decimal? GrossAmount { get; set; }
    public decimal? CreditAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? ExchangeRateToBrl { get; set; }
    public string ExchangeRateNature { get; set; } = "unknown";
    public string Source { get; set; } = string.Empty;
    public string RawData { get; set; } = string.Empty;
    public List<Guid> ExpenseExternalIds { get; set; } = new();
}
public sealed class CreateAwsInvoiceResponse { public Guid ExternalId { get; set; } }
