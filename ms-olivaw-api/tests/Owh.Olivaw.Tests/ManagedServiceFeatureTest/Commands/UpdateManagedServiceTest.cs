using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.ManagedServiceFeatureTest.Commands;

public class UpdateManagedServiceTest
{
    [Fact]
    public async Task Handle_should_end_removed_links_and_start_new_periods_atomically()
    {
        var service = ManagedServiceFakerBuilder.Create("customer");
        var previous = ManagedServiceFakerBuilder.Customer(1);
        var next = ManagedServiceFakerBuilder.Customer(2);
        var oldLink = ManagedServiceCustomer.Create(service, previous, ManagedServiceFakerBuilder.Actor, "Original", ManagedServiceFakerBuilder.Now);
        var repository = new Mock<IRepository<ManagedService>>();
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<ManagedService, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ManagedService, object>>[]>())).ReturnsAsync(service);
        var customers = new Mock<IRepository<Customer>>();
        customers.Setup(item => item.GetAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Customer, object>>[]>())).ReturnsAsync(new[] { next });
        var links = new Mock<IRepository<ManagedServiceCustomer>>();
        links.Setup(item => item.GetAsync(It.IsAny<Expression<Func<ManagedServiceCustomer, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ManagedServiceCustomer, object>>[]>())).ReturnsAsync(new[] { oldLink });
        var changes = new Mock<IRepository<ManagedServiceChange>>();
        var command = new UpdateManagedServiceMediatorCommand { ExternalId = service.ExternalId, ActorUserId = ManagedServiceFakerBuilder.Actor, ActorName = "Atualização", Payload = new() { Revision = 1, Name = "Novo nome", Type = "api", Status = "active", Scope = "customer", CustomerExternalIds = new() { next.ExternalId } } };
        var result = await new UpdateManagedServiceHandler(repository.Object, customers.Object, links.Object, changes.Object, Mock.Of<IRepositoryInventoryRevision>()).Handle(command, CancellationToken.None);
        Assert.Null(result.Conflict);
        Assert.Equal(2, result.Response.Revision);
        Assert.NotNull(oldLink.EndedAt);
        Assert.Equal("Original", oldLink.StartedByName);
        Assert.Equal("Atualização", oldLink.EndedByName);
        Assert.Equal(next.Id, Assert.Single(service.Customers).CustomerId);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        changes.Verify(item => item.AddAsync(It.Is<ManagedServiceChange>(change => change.Revision == 2), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull((await new UpdateManagedServiceHandler(repository.Object, customers.Object, links.Object, changes.Object, Mock.Of<IRepositoryInventoryRevision>()).Handle(command, CancellationToken.None)).Conflict);
    }
    [Fact]
    public async Task Handle_should_return_conflict_for_race_and_reject_null()
    {
        var service = ManagedServiceFakerBuilder.Create();
        var repository = new Mock<IRepository<ManagedService>>();
        repository.Setup(item => item.GetFirstAsync(It.IsAny<Expression<Func<ManagedService, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ManagedService, object>>[]>())).ReturnsAsync(service);
        repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException());
        var links = new Mock<IRepository<ManagedServiceCustomer>>();
        links.Setup(item => item.GetAsync(It.IsAny<Expression<Func<ManagedServiceCustomer, bool>>>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ManagedServiceCustomer, object>>[]>())).ReturnsAsync(Array.Empty<ManagedServiceCustomer>());
        var handler = new UpdateManagedServiceHandler(repository.Object, Mock.Of<IRepository<Customer>>(), links.Object, Mock.Of<IRepository<ManagedServiceChange>>(), Mock.Of<IRepositoryInventoryRevision>());
        var result = await handler.Handle(new() { ExternalId = service.ExternalId, ActorUserId = ManagedServiceFakerBuilder.Actor, ActorName = "Operador", Payload = new() { Revision = 1, Name = "Nome", Type = "api", Scope = "internal", Status = "closed" } }, CancellationToken.None);
        Assert.NotNull(result.Conflict);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
    [Fact]
    public void Validator_should_reject_closed_service_with_active_links_and_empty_revision()
    {
        var request = new Client.Features.ManagedServiceFeature.Commands.UpdateManagedServiceCommand { Revision = 1, Name = "Nome", Type = "api", Scope = "customer", Status = "closed" };
        var validator = new UpdateManagedServiceValidator();
        Assert.True(validator.Validate(request).IsValid);
        request.CustomerExternalIds.Add(Guid.NewGuid());
        Assert.False(validator.Validate(request).IsValid);
        request.CustomerExternalIds.Clear(); request.Revision = 0;
        Assert.False(validator.Validate(request).IsValid);
    }
}
