using System;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Client.Features.AwsInvoiceFeature.Commands;
namespace Owh.Olivaw.Tests.Mocks;
internal static class AwsInvoiceFakerBuilder
{
    internal static CreateAwsInvoiceCommand Payload()=>new(){SubmissionKey=Guid.NewGuid(),Issuer="AWS",AccountReference="Conta teste",BillingYear=2026,BillingMonth=10,GrossAmount=10,CreditAmount=10,TaxAmount=0,NetAmount=0,Source="Documento teste",RawData="{\"newField\":{\"items\":[1,2]}}"};
    internal static AwsInvoice Create(int id=1){var p=Payload();var e=AwsInvoice.Create(p.SubmissionKey,new string('A',64),p.Issuer,p.AccountReference,null,2026,10,"USD","provisional","partial",10,10,0,0,null,"unknown",p.Source,p.RawData,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);e.Id=id;return e;}
}
