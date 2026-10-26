using System;
namespace Owh.Olivaw.Client.Features.ExpenseFeature.Commands;
public sealed class CreateExpenseCommand
{
    public Guid EntryKey { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "infrastructure";
    public string Kind { get; set; } = "cost";
    public string Currency { get; set; } = "BRL";
    public decimal Amount { get; set; }
    public decimal ExchangeRateToBrl { get; set; } = 1;
    public DateTime OccurredAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public Guid? CustomerExternalId { get; set; }
}
public sealed class CreateExpenseResponse { public Guid ExternalId { get; set; } }
