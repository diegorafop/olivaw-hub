using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Api.Features.CustomerFeature.Commands;
using Owh.Olivaw.Client.Features.CustomerFeature.Commands;
using Xunit;

namespace Owh.Olivaw.Tests.CustomerFeatureTest.Commands;
public class UpdateCustomerTest
{
    [Theory]
    [InlineData(0, "active", false)]
    [InlineData(1, "invalid", false)]
    [InlineData(1, "closed", true)]
    public void Validator_should_require_revision_and_known_status(long revision, string status, bool valid)
    {
        var input = new UpdateCustomerCommand {Business="Empresa",Revision=revision,Status=status,Contacts=CustomerIntegrationTest.Input().Contacts};
        Assert.Equal(valid,new UpdateCustomerValidator().Validate(input).IsValid);
    }
    [Fact]
    public async Task Handle_should_reject_direct_reactivation_of_closed_customer()
    {
        await using var fixture = await CustomerFixture.StartAsync("root");
        fixture.Customer.Apply("Empresa", "", null, "", "", "closed", CustomerFixture.ActorId, "Operador", DateTime.UtcNow);
        var result = await new UpdateCustomerHandler(fixture.Repository.Object).Handle(new UpdateCustomerMediatorCommand {ExternalId=fixture.Customer.ExternalId,ActorUserId=CustomerFixture.ActorId,Payload=new UpdateCustomerCommand {Business="Empresa",Revision=2,Status="active",Contacts=CustomerIntegrationTest.Input().Contacts}},CancellationToken.None);
        Assert.NotNull(result.Conflict);
        fixture.Repository.Verify(item=>item.SaveChangesAsync(Moq.It.IsAny<CancellationToken>()),Moq.Times.Never);
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await using var fixture = await CustomerFixture.StartAsync("root");
        await Assert.ThrowsAsync<ArgumentNullException>(() => new UpdateCustomerHandler(fixture.Repository.Object).Handle(null!, CancellationToken.None));
    }
}
