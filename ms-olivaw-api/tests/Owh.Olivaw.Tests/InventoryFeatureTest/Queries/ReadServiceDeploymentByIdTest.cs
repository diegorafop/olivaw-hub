using System.Linq.Expressions;
using Owh.OlivawCore.Repositories;
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
public sealed class ReadServiceDeploymentByIdTest
{
    [Fact]
    public async Task Handle_should_map_snapshot_history_and_reject_missing_deployment()
    {
        var deployment = InventoryFakerBuilder.Deployment();
        var changes = new Mock<IRepository<ServiceDeploymentChange>>();
        changes.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<ServiceDeploymentChange, bool>>>(), 20, It.IsAny<Expression<Func<ServiceDeploymentChange, object>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ServiceDeploymentChange, object>>[]>())).ReturnsAsync(deployment.Changes);
        var handler = new ReadServiceDeploymentByIdHandler(InventoryFakerBuilder.Repository(deployment).Object, changes.Object);
        var request = new ReadServiceDeploymentByIdMediatorQuery { Payload = new() { ExternalId = deployment.ExternalId } };
        var response = await handler.Handle(request, CancellationToken.None);
        Assert.Equal("https://example.test", response.Endpoint); Assert.Equal("Operador", Assert.Single(response.History).ActorName);
        Assert.Equal(deployment.ManagedService.ExternalId, response.ServiceExternalId);
        Assert.True(new ReadServiceDeploymentByIdMediatorValidator().Validate(request).IsValid);
        request.Payload.ExternalId = Guid.Empty; Assert.False(new ReadServiceDeploymentByIdValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
