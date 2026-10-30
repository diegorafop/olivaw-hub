using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Api.Features.AwsInvoiceFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.AwsInvoiceFeatureTest.Commands;
public sealed class CreateAwsInvoiceTest
{
    [Fact] public async Task Handle_should_preserve_unknown_json_fields_reuse_evidence_and_reject_changed_key()
    {
        using var c=new CancellationTokenSource();var repo=InventoryFakerBuilder.Repository<AwsInvoice>();
        var p=AwsInvoiceFakerBuilder.Payload();var handler=new CreateAwsInvoiceHandler(repo.Object,InventoryFakerBuilder.Repository<Expense>().Object,InventoryFakerBuilder.Repository<AwsInvoiceExpense>().Object);
        var request=new CreateAwsInvoiceMediatorCommand{Payload=p,ActorUserId=InventoryFakerBuilder.Actor};
        Assert.True(new CreateAwsInvoiceMediatorValidator().Validate(request).IsValid);
        var first=await handler.Handle(request,c.Token);Assert.Null(first.Conflict);
        repo.Verify(x=>x.AddAsync(It.Is<AwsInvoice>(v=>v.RawData==p.RawData && v.GrossAmount==10 && v.NetAmount==0 && v.ActorUserId==InventoryFakerBuilder.Actor),c.Token),Times.Once);
        Assert.Equal(first.Response.ExternalId,(await handler.Handle(request,c.Token)).Response.ExternalId);
        p.GrossAmount=20;Assert.NotNull((await handler.Handle(request,c.Token)).Conflict);
        await Assert.ThrowsAsync<ArgumentNullException>(()=>handler.Handle(null!,c.Token));
    }
    [Theory][InlineData("[]")][InlineData("null")][InlineData("not json")]
    public void Validator_should_reject_non_object_json(string raw){var p=AwsInvoiceFakerBuilder.Payload();p.RawData=raw;Assert.False(new CreateAwsInvoiceValidator().Validate(p).IsValid);}
    [Fact] public void Validator_should_reject_inconsistent_totals_partial_payment_and_unknown_conversion()
    {
        var p=AwsInvoiceFakerBuilder.Payload();p.NetAmount=1;Assert.False(new CreateAwsInvoiceValidator().Validate(p).IsValid);
        p.NetAmount=0;p.DocumentStatus="paid";Assert.False(new CreateAwsInvoiceValidator().Validate(p).IsValid);
        p.DocumentStatus="provisional";p.ExchangeRateNature="payment";Assert.False(new CreateAwsInvoiceValidator().Validate(p).IsValid);
        p.ExchangeRateToBrl=5.2m;Assert.True(new CreateAwsInvoiceValidator().Validate(p).IsValid);
    }
    [Fact]public async Task Handle_should_link_existing_same_month_expense_and_block_reused_link()
    {
        var p=AwsInvoiceFakerBuilder.Payload();var expense=Expense.Create(Guid.NewGuid(),new string('B',64),"VM","infrastructure","cost","BRL",10,1,new DateTime(2026,10,1,0,0,0,DateTimeKind.Utc),"Fatura",null,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);expense.Id=7;p.ExpenseExternalIds.Add(expense.ExternalId);
        var invoices=InventoryFakerBuilder.Repository<AwsInvoice>();var expenses=InventoryFakerBuilder.Repository(expense);var links=InventoryFakerBuilder.Repository<AwsInvoiceExpense>();
        var handler=new CreateAwsInvoiceHandler(invoices.Object,expenses.Object,links.Object);var request=new CreateAwsInvoiceMediatorCommand{Payload=p,ActorUserId=InventoryFakerBuilder.Actor};
        Assert.Null((await handler.Handle(request,CancellationToken.None)).Conflict);
        invoices.Verify(x=>x.AddAsync(It.Is<AwsInvoice>(v=>v.Expenses.Single().ExpenseId==7),It.IsAny<CancellationToken>()),Times.Once);
        var previous=AwsInvoiceFakerBuilder.Create();previous.Link(expense,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);
        handler=new CreateAwsInvoiceHandler(InventoryFakerBuilder.Repository<AwsInvoice>().Object,expenses.Object,InventoryFakerBuilder.Repository(previous.Expenses.Single()).Object);
        Assert.NotNull((await handler.Handle(request,CancellationToken.None)).Conflict);
    }
}
