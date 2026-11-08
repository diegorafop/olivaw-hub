using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Api.Features.PlanFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.PlanFeatureTest.Commands;
public sealed class CreatePlanAssignmentTest
{
    [Fact]public async Task Handle_should_preserve_month_history_and_reject_competing_conditions()
    {
        var version=PlanVersion.Create(Guid.NewGuid(),new string('A',64),"bronze",1,49,2,64,0.1m,"Site estático","Teste",new(2026,10,1,0,0,0,DateTimeKind.Utc),null,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);version.Id=9;
        var customer=ManagedServiceFakerBuilder.Customer();var repo=InventoryFakerBuilder.Repository<CustomerPlan>();var h=new CreatePlanAssignmentHandler(repo.Object,InventoryFakerBuilder.Repository(customer).Object,InventoryFakerBuilder.Repository(version).Object);
        var q=new CreatePlanAssignmentMediatorCommand{ActorUserId=InventoryFakerBuilder.Actor,ActorName="Operador",Payload=new(){CustomerExternalId=customer.ExternalId,PlanVersionExternalId=version.ExternalId,Year=2026,Month=10,ApprovalReference="Teste"}};
        Assert.True(new CreatePlanAssignmentMediatorValidator().Validate(q).IsValid);var first=await h.Handle(q,CancellationToken.None);Assert.Null(first.Conflict);Assert.Equal(first.Response.ExternalId,(await h.Handle(q,CancellationToken.None)).Response.ExternalId);
        q.Payload.AgreedMonthlyPrice=39;Assert.NotNull((await h.Handle(q,CancellationToken.None)).Conflict);
        q.Payload.Month=11;Assert.Null((await h.Handle(q,CancellationToken.None)).Conflict);
        q.Payload.Month=9;await Assert.ThrowsAsync<ValidationException>(()=>h.Handle(q,CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(()=>h.Handle(null!,CancellationToken.None));
    }
}
