using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Api.Features.FinancialFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.FinancialFeatureTest.Commands;
public sealed class UpdateFinancialEntryTest
{
 [Fact]public async Task Handle_should_cancel_with_reason_and_guard_revision()
 {
  var e=FinancialFakerBuilder.Entry();var h=new UpdateFinancialEntryHandler(InventoryFakerBuilder.Repository(e).Object);var q=new UpdateFinancialEntryMediatorCommand{ActorUserId=InventoryFakerBuilder.Actor,Payload=new(){ExternalId=e.ExternalId,Revision=1,CancellationReason="Cancelado pelo cliente"}};
  Assert.True(new UpdateFinancialEntryMediatorValidator().Validate(q).IsValid);Assert.Null((await h.Handle(q,CancellationToken.None)).Conflict);Assert.Equal("cancelled",e.Status);Assert.NotNull((await h.Handle(q,CancellationToken.None)).Conflict);
 }
}
