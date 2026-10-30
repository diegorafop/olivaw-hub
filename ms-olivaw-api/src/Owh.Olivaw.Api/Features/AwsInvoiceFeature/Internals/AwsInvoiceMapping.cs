using Owh.Olivaw.Domain;
using Owh.Olivaw.Client.Features.AwsInvoiceFeature.Queries;
namespace Owh.Olivaw.Api.Features.AwsInvoiceFeature.Internals;
internal static class AwsInvoiceMapping
{
    internal static AwsInvoiceItem ToItem(this AwsInvoice x)=>new(){ExternalId=x.ExternalId,Issuer=x.Issuer,AccountReference=x.AccountReference,InvoiceNumber=x.InvoiceNumber,BillingYear=x.BillingYear,BillingMonth=x.BillingMonth,Currency=x.Currency,DocumentStatus=x.DocumentStatus,Completeness=x.Completeness,GrossAmount=x.GrossAmount,CreditAmount=x.CreditAmount,TaxAmount=x.TaxAmount,NetAmount=x.NetAmount,ExchangeRateToBrl=x.ExchangeRateToBrl,ExchangeRateNature=x.ExchangeRateNature,Source=x.Source,CreatedAt=x.CreatedAt};
}
