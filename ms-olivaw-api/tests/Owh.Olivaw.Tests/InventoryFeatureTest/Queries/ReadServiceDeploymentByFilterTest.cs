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
public sealed class ReadServiceDeploymentByFilterTest
{
    [Fact]
    public async Task Handle_should_map_service_and_environment_and_preserve_page()
    {
        using var cancellation = new CancellationTokenSource(); var deployment = InventoryFakerBuilder.Deployment();
        var repository = new Mock<IRepositoryServiceDeployment>();
        repository.Setup(item => item.ReadPageAsync(null, "", 2, 20, cancellation.Token)).ReturnsAsync(new ServiceDeploymentPage { Total = 21, Items = new() {deployment} });
        var handler = new ReadServiceDeploymentByFilterHandler(repository.Object);
        var request = new ReadServiceDeploymentByFilterMediatorQuery { Payload = new() { Page = 2 } };
        var result = await handler.Handle(request, cancellation.Token);
        Assert.Equal(21, result.Total); Assert.Equal(deployment.ManagedService.ExternalId, Assert.Single(result.Items).ServiceExternalId);
        Assert.Equal("production", result.Items[0].Environment); repository.VerifyAll();
        Assert.True(new ReadServiceDeploymentByFilterMediatorValidator().Validate(request).IsValid);
        request.Payload.PageSize = 101; Assert.False(new ReadServiceDeploymentByFilterValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
}
