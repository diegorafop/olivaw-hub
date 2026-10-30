using System;
using System.Collections.Generic;
namespace Owh.Olivaw.Client.Features.FinancialFeature.Queries;
public sealed class ReadFinancialAccountByFilterQuery { public int BeforeId{get;set;} }
public sealed class ReadFinancialAccountByFilterResponse { public List<FinancialAccountItem> Items{get;set;}=new();public int? NextBeforeId{get;set;} }
public sealed class FinancialAccountItem { public Guid ExternalId{get;set;}public string Name{get;set;}=string.Empty;public decimal OpeningBalance{get;set;}public DateTime OpenedAt{get;set;} }
public sealed class ReadFinancialEntryByFilterQuery { public DateTime From{get;set;}public DateTime To{get;set;}public Guid? CustomerExternalId{get;set;}public string? Direction{get;set;}public string? Status{get;set;}public int BeforeId{get;set;} }
public sealed class ReadFinancialEntryByFilterResponse { public List<FinancialEntryItem> Items{get;set;}=new();public int? NextBeforeId{get;set;} }
public sealed class ReadFinancialEntryByIdQuery { public Guid ExternalId{get;set;} }
public sealed class ReadFinancialEntryByIdResponse { public FinancialEntryItem Entry{get;set;}=new();public List<FinancialSettlementItem> Settlements{get;set;}=new(); }
public sealed class FinancialEntryItem { public Guid ExternalId{get;set;}public string Description{get;set;}=string.Empty;public string Direction{get;set;}=string.Empty;public string Category{get;set;}=string.Empty;public decimal Amount{get;set;}public decimal SettledAmount{get;set;}public DateTime DueAt{get;set;}public string Source{get;set;}=string.Empty;public string Status{get;set;}=string.Empty;public string CancellationReason{get;set;}=string.Empty;public string CustomerName{get;set;}=string.Empty;public Guid? CustomerExternalId{get;set;}public Guid? ExpenseExternalId{get;set;}public long Revision{get;set;} }
public sealed class FinancialSettlementItem {public Guid ExternalId{get;set;}public string AccountName{get;set;}=string.Empty;public decimal Amount{get;set;}public bool IsRefund{get;set;}public DateTime PaidAt{get;set;}public string Source{get;set;}=string.Empty;}
public sealed class ReadFinancialSummaryQuery {public DateTime From{get;set;}public DateTime To{get;set;}public Guid? CustomerExternalId{get;set;} }
public sealed class ReadFinancialSummaryResponse {public decimal ReceivableBrl{get;set;}public decimal PayableBrl{get;set;}public decimal ReceivedBrl{get;set;}public decimal PaidBrl{get;set;}public decimal OpeningBalanceBrl{get;set;}public decimal ClosingBalanceBrl{get;set;}public List<FinancialFlowPoint> Series{get;set;}=new();public bool HasAccounts{get;set;}public bool IsCustomerContribution{get;set;} }
public sealed class FinancialFlowPoint {public DateTime Date{get;set;}public decimal ReceivedBrl{get;set;}public decimal PaidBrl{get;set;} }
