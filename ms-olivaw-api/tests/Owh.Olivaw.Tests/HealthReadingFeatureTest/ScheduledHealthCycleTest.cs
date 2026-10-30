using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Owh.Olivaw.Api.Services;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.HealthReadingFeatureTest;
public sealed class ScheduledHealthCycleTest
{
    private sealed class ResponseHandler : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));}
    }
    [Fact]public void Sample_key_should_deduplicate_same_window_without_colliding_between_resources()
    {
        var id=Guid.NewGuid();var now=new DateTime(2026,10,30,12,0,0,DateTimeKind.Utc);
        Assert.Equal(ScheduledHealthCycle.SampleKey(id,now,15),ScheduledHealthCycle.SampleKey(id,now.AddMinutes(14),15));
        Assert.NotEqual(ScheduledHealthCycle.SampleKey(id,now,15),ScheduledHealthCycle.SampleKey(id,now.AddMinutes(15),15));
        Assert.NotEqual(ScheduledHealthCycle.SampleKey(id,now,15),ScheduledHealthCycle.SampleKey(Guid.NewGuid(),now,15));
    }
    [Fact]public async Task Cycle_should_collect_only_authorized_resource_record_status_and_never_duplicate_window()
    {
        var resource=InventoryResource.Create("public-health",new(){Name="Site",Type="compute",ReferenceUrl="https://health.example.test/ready"},InventoryFakerBuilder.Actor,"Operador",InventoryFakerBuilder.Now);resource.Id=7;
        var rows=InventoryFakerBuilder.Repository<HealthReading>();var transport=new ResponseHandler();using var http=new HttpClient(transport);
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{["Monitoring:AllowedUrls:0"]=resource.ReferenceUrl}).Build();
        var status=new HealthCollectionStatus();var cycle=new ScheduledHealthCycle(InventoryFakerBuilder.Repository(resource).Object,rows.Object,new PublicHealthProbe(http,config),status,NullLogger<ScheduledHealthCycle>.Instance);
        var options=new ScheduledHealthOptions{Enabled=true,ActorUserId=InventoryFakerBuilder.Actor,ResourceExternalIds=new[]{resource.ExternalId}};
        await cycle.RunAsync(options,CancellationToken.None);
        Assert.Equal(1,transport.Calls);Assert.Equal(0,status.Read().Failures);Assert.Equal(1,status.Read().Attempted);
        rows.Verify(x=>x.AddAsync(It.Is<HealthReading>(v=>v.InventoryResourceId==7 && v.Source==ScheduledHealthCycle.Source && v.Outcome=="success" && v.MaxAgeMinutes==30),It.IsAny<CancellationToken>()),Times.Once);
        options.ResourceExternalIds=new[]{Guid.NewGuid()};await cycle.RunAsync(options,CancellationToken.None);Assert.Equal(1,transport.Calls);Assert.Equal(1,status.Read().Failures);
        options.Enabled=false;await cycle.RunAsync(options,CancellationToken.None);Assert.Equal(1,transport.Calls);
    }
    [Theory][InlineData(1,30)][InlineData(15,1)][InlineData(61,90)]public void Options_should_reject_unbounded_or_too_frequent_collection(int minutes,int days)
    {
        var options=new ScheduledHealthOptions{Enabled=true,IntervalMinutes=minutes,RetentionDays=days,ActorUserId=InventoryFakerBuilder.Actor,ResourceExternalIds=new[]{Guid.NewGuid()}};Assert.Throws<InvalidOperationException>(options.Validate);
    }
}
