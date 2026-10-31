using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Api.Features.FinancialFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.FinancialFeatureTest.Commands;
public sealed class CreateFinancialSettlementTest
{
 [Fact]public async Task Handle_should_reuse_payment_key_reject_stale_revision_and_guard_balance()
 {
  var e=FinancialFakerBuilder.Entry();var a=FinancialFakerBuilder.Account();var settlements=InventoryFakerBuilder.Repository<FinancialSettlement>();var entries=InventoryFakerBuilder.Repository(e);var handler=new CreateFinancialSettlementHandler(settlements.Object,entries.Object,InventoryFakerBuilder.Repository(a).Object);
  var p=new CreateFinancialSettlementMediatorCommand{ActorUserId=InventoryFakerBuilder.Actor,Payload=new(){SettlementKey=Guid.NewGuid(),EntryExternalId=e.ExternalId,AccountExternalId=a.ExternalId,Amount=20,PaidAt=FinancialFakerBuilder.Day,Source="Comprovante teste",Revision=1}};
  Assert.True(new CreateFinancialSettlementMediatorValidator().Validate(p).IsValid);var first=await handler.Handle(p,CancellationToken.None);Assert.Null(first.Conflict);Assert.Equal(20,e.SettledAmount);
  Assert.Equal(first.Response.ExternalId,(await handler.Handle(p,CancellationToken.None)).Response.ExternalId);Assert.Equal(20,e.SettledAmount);
  p.Payload.Amount=21;Assert.NotNull((await handler.Handle(p,CancellationToken.None)).Conflict);
  p.Payload.SettlementKey=Guid.NewGuid();p.Payload.Amount=30;Assert.NotNull((await handler.Handle(p,CancellationToken.None)).Conflict);
  p.Payload.Revision=e.Revision;Assert.Null((await handler.Handle(p,CancellationToken.None)).Conflict);Assert.Equal("settled",e.Status);
  settlements.Verify(x=>x.SaveChangesAsync(It.IsAny<CancellationToken>()),Times.Exactly(2));
 }
}
