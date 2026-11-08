using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Api.Features.FinancialFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.FinancialFeatureTest.Commands;
public sealed class CreateFinancialAccountTest
{
 [Fact]public async Task Handle_should_preserve_opening_balance_and_idempotency()
 {
  var h=new CreateFinancialAccountHandler(InventoryFakerBuilder.Repository<FinancialAccount>().Object);var q=new CreateFinancialAccountMediatorCommand{ActorUserId=InventoryFakerBuilder.Actor,Payload=new(){AccountKey=Guid.NewGuid(),Name="Conta operacional",OpeningBalance=100,OpenedAt=FinancialFakerBuilder.Day}};
  Assert.True(new CreateFinancialAccountMediatorValidator().Validate(q).IsValid);var first=await h.Handle(q,CancellationToken.None);Assert.Equal(first.Response.ExternalId,(await h.Handle(q,CancellationToken.None)).Response.ExternalId);q.Payload.OpeningBalance=200;Assert.NotNull((await h.Handle(q,CancellationToken.None)).Conflict);await Assert.ThrowsAsync<ArgumentNullException>(()=>h.Handle(null!,CancellationToken.None));
 }
}
