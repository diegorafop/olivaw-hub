using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Api.Features.FinancialFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.FinancialFeatureTest.Commands;
public sealed class CreateFinancialTransferTest
{
 [Fact]public async Task Handle_should_record_internal_transfer_once_and_reject_same_account()
 {
  var a=FinancialFakerBuilder.Account();var b=FinancialFakerBuilder.Account(2);var h=new CreateFinancialTransferHandler(InventoryFakerBuilder.Repository<FinancialTransfer>().Object,InventoryFakerBuilder.Repository(a,b).Object);
  var q=new CreateFinancialTransferMediatorCommand{ActorUserId=InventoryFakerBuilder.Actor,Payload=new(){TransferKey=Guid.NewGuid(),FromAccountExternalId=a.ExternalId,ToAccountExternalId=b.ExternalId,Amount=30,TransferredAt=FinancialFakerBuilder.Day,Source="Comprovante teste"}};
  Assert.True(new CreateFinancialTransferMediatorValidator().Validate(q).IsValid);var first=await h.Handle(q,CancellationToken.None);Assert.Equal(first.Response.ExternalId,(await h.Handle(q,CancellationToken.None)).Response.ExternalId);q.Payload.ToAccountExternalId=a.ExternalId;Assert.False(new CreateFinancialTransferValidator().Validate(q.Payload).IsValid);
 }
}
