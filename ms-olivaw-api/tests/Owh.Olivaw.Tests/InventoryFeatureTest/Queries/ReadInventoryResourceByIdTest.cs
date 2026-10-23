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
public sealed class ReadInventoryResourceByIdTest
{
    [Fact]
    public async Task Handle_should_map_capacity_audit_and_consumer_context_without_writes()
    {
        var resource = InventoryFakerBuilder.Resource(); var repository = InventoryFakerBuilder.Repository(resource);
        var graph = new Mock<IRepositoryInventoryResource>();
        graph.Setup(item => item.ReadContextAsync(resource.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new InventoryResourceContext { CustomerCount = 2, ReservedCpu = 3, IncompleteReservations = 1 });
        var changes = new Mock<IRepository<InventoryResourceChange>>();
        changes.Setup(item => item.TakeOrderedAsync(It.IsAny<Expression<Func<InventoryResourceChange, bool>>>(), 20, It.IsAny<Expression<Func<InventoryResourceChange, object>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<InventoryResourceChange, object>>[]>())).ReturnsAsync(resource.Changes);
        var handler = new ReadInventoryResourceByIdHandler(repository.Object, graph.Object, changes.Object);
        var request = new ReadInventoryResourceByIdMediatorQuery { Payload = new() { ExternalId = resource.ExternalId } };
        var response = await handler.Handle(request, CancellationToken.None);
        Assert.Equal(resource.TechnicalId, response.TechnicalId); Assert.Equal(2, response.CustomerCount); Assert.Equal(3, response.ReservedCpu); Assert.Null(response.MemoryCapacityGiB); Assert.Single(response.History);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.True(new ReadInventoryResourceByIdMediatorValidator().Validate(request).IsValid);
        request.Payload.ExternalId = Guid.Empty; Assert.False(new ReadInventoryResourceByIdValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
