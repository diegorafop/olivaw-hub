using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Api.Features.CustomerFeature.Commands;
using Owh.Olivaw.Client.Features.CustomerFeature.Commands;
using Xunit;

namespace Owh.Olivaw.Tests.CustomerFeatureTest.Commands;
public class CreateCustomerTest
{
    [Theory]
    [InlineData("11987654321", "", true)]
    [InlineData("", "maria@example.test", true)]
    [InlineData("11987654321", "bad-email", false)]
    [InlineData("123", "maria@example.test", false)]
    [InlineData("", "", false)]
    public void Validator_should_accept_at_least_one_valid_channel_and_reject_invalid_second_channel(string whatsapp, string email, bool valid)
    {
        var input = CustomerIntegrationTest.Input(); input.Contacts[0].Whatsapp = whatsapp; input.Contacts[0].Email = email;
        Assert.Equal(valid, new CreateCustomerValidator().Validate(input).IsValid);
    }
    [Fact]
    public async Task Handle_should_propagate_cancellation_and_keep_null_source()
    {
        await using var fixture = await CustomerFixture.StartAsync("root");
        using var source = new CancellationTokenSource();
        var result = await new CreateCustomerHandler(fixture.Repository.Object, new Moq.Mock<Owh.OlivawCore.Repositories.IRepository<Owh.Olivaw.Domain.Lead>>().Object).Handle(new CreateCustomerMediatorCommand {Payload = CustomerIntegrationTest.Input(), ActorUserId = CustomerFixture.ActorId, ActorName = "Operador"}, source.Token);
        Assert.Null(result.Conflict);
        fixture.Repository.Verify(item => item.SaveChangesAsync(source.Token), Moq.Times.Once);
        Assert.Null(fixture.Items.Find(item => item.ExternalId == result.Response.ExternalId)?.SourceLeadId);
    }
    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await using var fixture = await CustomerFixture.StartAsync("root");
        await Assert.ThrowsAsync<ArgumentNullException>(() => new CreateCustomerHandler(fixture.Repository.Object, new Moq.Mock<Owh.OlivawCore.Repositories.IRepository<Owh.Olivaw.Domain.Lead>>().Object).Handle(null!, CancellationToken.None));
    }
}
