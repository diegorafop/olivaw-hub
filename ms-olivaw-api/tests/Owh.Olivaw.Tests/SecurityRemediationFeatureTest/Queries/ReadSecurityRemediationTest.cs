using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Api.Features.SecurityRemediationFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.SecurityRemediationFeatureTest.Queries;
public sealed class ReadSecurityRemediationTest
{
    [Fact]
    public async Task Read_should_supply_review_plans_preserve_unknown_fields_and_hide_lease()
    {
        var report = SecurityRemediationFakerBuilder.Report();var ticket = SecurityRemediationFakerBuilder.Ticket("executing");
        var response = await new ReadSecurityRemediationHandler(InventoryFakerBuilder.Repository(report).Object, InventoryFakerBuilder.Repository(ticket).Object)
            .Handle(new() { Payload = new() { ReportExternalId = report.ExternalId } }, CancellationToken.None);
        Assert.Equal(2, response.Items.Count);Assert.Equal("executing", response.Items[0].Status);Assert.False(response.Items[1].Supported);
        Assert.DoesNotContain("LeaseToken", System.Text.Json.JsonSerializer.Serialize(response));
    }
    [Fact]
    public async Task Queue_should_only_return_own_unexpired_authorizations_for_the_requested_host()
    {
        var own = SecurityRemediationFakerBuilder.Ticket("authorized");
        var other = SecurityRemediationFakerBuilder.Ticket("authorized");other.ExecutorUserId = Guid.NewGuid();
        var expired = SecurityRemediationFakerBuilder.Ticket("authorized");expired.ApprovalExpiresAt = DateTime.UtcNow.AddDays(-1);
        var response = await new ReadSecurityRemediationQueueHandler(InventoryFakerBuilder.Repository(own, other, expired).Object)
            .Handle(new() { ActorUserId = own.ExecutorUserId, HostId = own.HostId }, CancellationToken.None);
        Assert.Single(response.Items);Assert.Equal(own.ExternalId, response.Items[0].ExternalId);
        Assert.False(new ReadSecurityRemediationQueueValidator().Validate(new ReadSecurityRemediationQueueMediatorQuery()).IsValid);
    }
}
