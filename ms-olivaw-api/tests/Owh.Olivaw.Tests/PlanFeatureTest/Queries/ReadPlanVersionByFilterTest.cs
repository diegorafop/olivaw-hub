using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Api.Features.PlanFeature.Queries;
using Xunit;
namespace Owh.Olivaw.Tests.PlanFeatureTest.Queries;
public sealed class ReadPlanVersionByFilterTest
{
    [Fact]public async Task Handle_should_distinguish_current_future_and_missing_costs()
    {
        var now=DateTime.UtcNow;var month=new DateTime(now.Year,now.Month,1,0,0,0,DateTimeKind.Utc);
        PlanVersion Make(int number,DateTime date){var v=PlanVersion.Create(Guid.NewGuid(),new string('A',64),"bronze",number,49,2,null,null,"Estático","Teste",date,null,InventoryFakerBuilder.Actor,now);v.Id=number;return v;}
        var current=Make(1,month);var future=Make(2,month.AddMonths(1));var h=new ReadPlanVersionByFilterHandler(InventoryFakerBuilder.Repository(current,future).Object);
        var q=new ReadPlanVersionByFilterMediatorQuery{Payload=new(){PlanKey="bronze"}};var result=await h.Handle(q,CancellationToken.None);
        Assert.Equal(current.ExternalId,result.Current!.ExternalId);Assert.Equal(2,result.Items.Count);Assert.Null(result.Current.EstimatedMonthlyCost);
        q.Payload.PlanKey="prata";Assert.Empty((await h.Handle(q,CancellationToken.None)).Items);
        q.Payload.PlanKey="outro";Assert.False(new ReadPlanVersionByFilterValidator().Validate(q.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(()=>h.Handle(null!,CancellationToken.None));
    }
}
