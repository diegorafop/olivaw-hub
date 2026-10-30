using System;
namespace Owh.Olivaw.Client.Features.FinancialFeature.Commands;
public sealed class CreateFinancialAccountCommand { public Guid AccountKey{get;set;} public string Name{get;set;}=string.Empty; public decimal OpeningBalance{get;set;} public DateTime OpenedAt{get;set;} }
public sealed class CreateFinancialAccountResponse { public Guid ExternalId{get;set;} }
public sealed class CreateFinancialEntryCommand { public Guid EntryKey{get;set;} public string Description{get;set;}=string.Empty; public string Direction{get;set;}="payable"; public string Category{get;set;}="other";public decimal Amount{get;set;}public DateTime DueAt{get;set;}public string Source{get;set;}=string.Empty;public Guid? CustomerExternalId{get;set;}public Guid? ExpenseExternalId{get;set;} }
public sealed class CreateFinancialEntryResponse { public Guid ExternalId{get;set;} }
public sealed class CreateFinancialSettlementCommand { public Guid SettlementKey{get;set;}public Guid EntryExternalId{get;set;}public Guid AccountExternalId{get;set;}public decimal Amount{get;set;}public bool IsRefund{get;set;}public DateTime PaidAt{get;set;}public string Source{get;set;}=string.Empty;public long Revision{get;set;} }
public sealed class CreateFinancialSettlementResponse { public Guid ExternalId{get;set;}public long Revision{get;set;} }
public sealed class CreateFinancialTransferCommand { public Guid TransferKey{get;set;}public Guid FromAccountExternalId{get;set;}public Guid ToAccountExternalId{get;set;}public decimal Amount{get;set;}public DateTime TransferredAt{get;set;}public string Source{get;set;}=string.Empty; }
public sealed class CreateFinancialTransferResponse { public Guid ExternalId{get;set;} }
public sealed class UpdateFinancialEntryCommand { public Guid ExternalId{get;set;}public long Revision{get;set;}public string CancellationReason{get;set;}=string.Empty; }
public sealed class UpdateFinancialEntryResponse { public Guid ExternalId{get;set;}public long Revision{get;set;} }
