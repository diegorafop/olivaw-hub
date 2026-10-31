using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.ExpenseFeature.Queries;
public sealed class ReadExpenseByFilterQuery
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string GroupBy { get; set; } = "day";
    public Guid? CustomerExternalId { get; set; }
    public bool UnallocatedOnly { get; set; }
    public bool IncludeCredits { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
public sealed class ReadExpenseByFilterResponse
{
    public decimal GrossBrl { get; set; }
    public decimal CreditsBrl { get; set; }
    public decimal NetBrl { get; set; }
    public decimal UnallocatedBrl { get; set; }
    public List<ExpensePoint> Series { get; set; } = new();
    public List<ExpenseItem> Items { get; set; } = new();
    public int Total { get; set; }
    public List<ExpenseBreakdown> ByCategory { get; set; } = new();
    public List<ExpenseBreakdown> ByCustomer { get; set; } = new();
}
public sealed class ExpensePoint { public DateTime Date { get; set; } public decimal CostBrl { get; set; } public decimal CreditsBrl { get; set; } public decimal NetBrl { get; set; } }
public sealed class ExpenseItem
{
    public Guid ExternalId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRateToBrl { get; set; }
    public decimal AmountBrl { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}

public sealed class ExpenseBreakdown
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal GrossBrl { get; set; }
    public decimal CreditsBrl { get; set; }
    public decimal NetBrl { get; set; }
    public int Entries { get; set; }
}
