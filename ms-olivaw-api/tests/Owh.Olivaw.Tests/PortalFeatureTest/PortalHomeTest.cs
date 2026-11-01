using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Api.Features.PortalFeature.Commands;
using Owh.Olivaw.Api.Features.PortalFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.PortalFeatureTest;
public sealed class PortalHomeTest
{
    [Fact] public async Task Home_should_scope_samples_and_keep_missing_days_null_without_counting_shared_VM_usage()
    {
        var now=DateTime.UtcNow;var day=now.Date.AddDays(-1);var own=ManagedServiceFakerBuilder.Customer();var other=ManagedServiceFakerBuilder.Customer(2);
        var sample=PortalUsageSample.Create(own,Guid.NewGuid(),day,512,12,"Estatística consolidada desta empresa",InventoryFakerBuilder.Actor,now);
        var foreign=PortalUsageSample.Create(other,Guid.NewGuid(),day,999,999,"Outra empresa",InventoryFakerBuilder.Actor,now);
        var plan=CustomerPlan.Create(own,"Bronze",299,null,2,"Escopo","Aprovação",day,InventoryFakerBuilder.Actor,"Operador",now);
        var handler=new ReadPortalHomeHandler(null!,InventoryFakerBuilder.Repository(own,other).Object,InventoryFakerBuilder.Repository(sample,foreign).Object,InventoryFakerBuilder.Repository<SupportRequest>().Object,InventoryFakerBuilder.Repository(plan).Object);
        var response=await handler.Handle(new(){Preview=true,ActorUserId=InventoryFakerBuilder.Actor,Payload=new(){CustomerExternalId=own.ExternalId,Period="week"}},CancellationToken.None);
        Assert.Equal(7,response.Series.Count);Assert.Null(response.Series[0].PageViews);Assert.Equal(12,response.PageViews);Assert.Equal(1,response.ObservedDays);Assert.Equal(2048,response.StorageLimitMiB);Assert.Equal(25,response.StoragePercent);Assert.Equal(512,response.StorageUsedMiB);
    }
    [Fact]public void Samples_should_reject_unfinished_days_and_keep_zero_distinct_from_missing()
    {
        var now=DateTime.UtcNow;var customer=ManagedServiceFakerBuilder.Customer();
        Assert.Throws<ArgumentException>(()=>PortalUsageSample.Create(customer,Guid.NewGuid(),now.Date,1,2,"Fonte",InventoryFakerBuilder.Actor,now));
        Assert.Throws<ArgumentException>(()=>PortalUsageSample.Create(customer,Guid.NewGuid(),now.Date.AddDays(-1),null,null,"Fonte",InventoryFakerBuilder.Actor,now));
        var zero=PortalUsageSample.Create(customer,Guid.NewGuid(),now.Date.AddDays(-1),0,null,"Fonte",InventoryFakerBuilder.Actor,now);
        Assert.Equal(0,zero.StorageUsedMiB);Assert.Null(zero.PageViews);
        Assert.True(new CreatePortalUsageSampleValidator().Validate(new Owh.Olivaw.Client.Features.PortalFeature.Commands.CreatePortalUsageSampleCommand(){CustomerExternalId=customer.ExternalId,SampleKey=Guid.NewGuid(),Day=now.Date.AddDays(-1),StorageUsedMiB=0,Source="Fonte"}).IsValid);
    }
}
