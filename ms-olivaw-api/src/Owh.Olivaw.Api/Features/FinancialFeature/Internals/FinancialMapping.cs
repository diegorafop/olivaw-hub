using Owh.Olivaw.Domain;
using Owh.Olivaw.Client.Features.FinancialFeature.Queries;
namespace Owh.Olivaw.Api.Features.FinancialFeature.Internals;
internal static class FinancialMapping
{
 internal static FinancialEntryItem Map(this FinancialEntry x)=>new(){ExternalId=x.ExternalId,Description=x.Description,Direction=x.Direction,Category=x.Category,Amount=x.Amount,SettledAmount=x.SettledAmount,DueAt=x.DueAt,Source=x.Source,Status=x.Status,CancellationReason=x.CancellationReason,CustomerName=x.Customer?.Business ?? "Compartilhada / sem cliente",CustomerExternalId=x.Customer?.ExternalId,ExpenseExternalId=x.Expense?.ExternalId,Revision=x.Revision};
}
