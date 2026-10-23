using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Owh.Olivaw.Api.Features.InventoryFeature.Queries;
using Owh.Olivaw.Client.Features.InventoryFeature.Queries;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.InventoryFeatureTest.Queries;
public sealed class ReadInventoryLinkByFilterTest
{
    [Fact]
    public async Task Handle_should_filter_periods_and_map_reservation_and_authors()
    {
        var resource = InventoryFakerBuilder.Resource(); var repository = new Mock<IRepositoryInventoryResource>();
        using var cancellation = new CancellationTokenSource();
        repository.Setup(item => item.ReadLinksAsync(It.Is<InventoryLinkFilter>(filter => filter.ResourceExternalId == resource.ExternalId && filter.IncludeEnded && filter.Kind == "deployment"), cancellation.Token)).ReturnsAsync(new InventoryLinkPage { Total = 1, Items = new() {new() { ResourceExternalId = resource.ExternalId, StartedByName = "Início", EndedByName = "Fim", ReservedCpu = 2 }} });
        var handler = new ReadInventoryLinkByFilterHandler(repository.Object, InventoryFakerBuilder.Repository(resource).Object, InventoryFakerBuilder.Repository<ServiceDeployment>().Object);
        var request = new ReadInventoryLinkByFilterMediatorQuery { Payload = new() { ResourceExternalId = resource.ExternalId, Kind = "deployment", IncludeEnded = true } };
        var response = await handler.Handle(request, cancellation.Token);
        Assert.Equal(1, response.Total); Assert.Equal("Fim", Assert.Single(response.Items).EndedByName); Assert.Equal(2, response.Items[0].ReservedCpu);
        Assert.True(new ReadInventoryLinkByFilterMediatorValidator().Validate(request).IsValid);
        request.Payload.DeploymentExternalId = Guid.NewGuid(); Assert.False(new ReadInventoryLinkByFilterValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
        repository.VerifyAll();
    }
}
