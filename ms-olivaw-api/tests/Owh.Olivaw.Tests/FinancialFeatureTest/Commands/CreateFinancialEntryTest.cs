using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Api.Features.FinancialFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.FinancialFeatureTest.Commands;
public sealed class CreateFinancialEntryTest
{
 [Fact]public async Task Handle_should_not_create_expense_or_payment_and_reject_changed_key()
 {
  var h=new CreateFinancialEntryHandler(InventoryFakerBuilder.Repository<FinancialEntry>().Object,InventoryFakerBuilder.Repository<Customer>().Object,InventoryFakerBuilder.Repository<Expense>().Object);var q=new CreateFinancialEntryMediatorCommand{ActorUserId=InventoryFakerBuilder.Actor,Payload=new(){EntryKey=Guid.NewGuid(),Description="Mensalidade",Direction="receivable",Amount=49,DueAt=FinancialFakerBuilder.Day,Source="Contrato teste"}};
  Assert.True(new CreateFinancialEntryMediatorValidator().Validate(q).IsValid);var first=await h.Handle(q,CancellationToken.None);Assert.Equal(first.Response.ExternalId,(await h.Handle(q,CancellationToken.None)).Response.ExternalId);q.Payload.Amount=59;Assert.NotNull((await h.Handle(q,CancellationToken.None)).Conflict);await Assert.ThrowsAsync<ArgumentNullException>(()=>h.Handle(null!,CancellationToken.None));
 }
}
