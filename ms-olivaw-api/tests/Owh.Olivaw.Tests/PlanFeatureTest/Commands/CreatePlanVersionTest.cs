using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Api.Features.PlanFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.PlanFeatureTest.Commands;
public sealed class CreatePlanVersionTest
{
    [Fact]public async Task Handle_should_publish_immutable_versions_idempotently_without_updating_customers()
    {
        var repo=InventoryFakerBuilder.Repository<PlanVersion>();var h=new CreatePlanVersionHandler(repo.Object,InventoryFakerBuilder.Repository<PlanStudy>().Object);
        var q=new CreatePlanVersionMediatorCommand{ActorUserId=InventoryFakerBuilder.Actor,Payload=new(){PublicationKey=Guid.NewGuid(),PlanKey="bronze",MonthlyPrice=49,StorageLimitGiB=2,Conditions="Site estático",ApprovalReference="Teste",EffectiveAt=new(2026,10,1,0,0,0,DateTimeKind.Utc)}};
        Assert.True(new CreatePlanVersionMediatorValidator().Validate(q).IsValid);var first=await h.Handle(q,CancellationToken.None);Assert.Equal(1,first.Response.Version);
        Assert.Equal(first.Response.ExternalId,(await h.Handle(q,CancellationToken.None)).Response.ExternalId);
        q.Payload.MonthlyPrice=59;Assert.NotNull((await h.Handle(q,CancellationToken.None)).Conflict);
        q.Payload.PublicationKey=Guid.NewGuid();q.Payload.EffectiveAt=q.Payload.EffectiveAt.AddMonths(1);Assert.Equal(2,(await h.Handle(q,CancellationToken.None)).Response.Version);
        q.Payload.EffectiveAt=q.Payload.EffectiveAt.AddDays(1);Assert.False(new CreatePlanVersionValidator().Validate(q.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(()=>h.Handle(null!,CancellationToken.None));
    }
}
