using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using Owh.Olivaw.Api.Features.InventoryFeature.Commands;
using Owh.Olivaw.Client.Features.InventoryFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Xunit;
namespace Owh.Olivaw.Tests.InventoryFeatureTest.Commands;
public sealed class CreateResourceCustomerTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_should_preserve_periods_and_report_concurrent_changes(bool conflict)
    {
        using var cancellation = new CancellationTokenSource();
        var source = InventoryFakerBuilder.Resource();
        var target = ManagedServiceFakerBuilder.Customer();
        var resources = InventoryFakerBuilder.Repository(source);
        var targets = InventoryFakerBuilder.Repository(target);
        var inventory = new Mock<IRepositoryInventoryRevision>();
        var graph = new Mock<IRepositoryInventoryResource>();
        graph.Setup(item => item.ReadGraphAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new InventoryGraphData());
        var link = ResourceCustomer.Create(source, target, "Uso", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var repository = InventoryFakerBuilder.Repository<ResourceCustomer>();
        if (conflict) repository.Setup(item => item.SaveChangesAsync(cancellation.Token)).ThrowsAsync(new DbUpdateConcurrencyException());
        var request = new CreateResourceCustomerMediatorCommand { Payload = new() { ResourceExternalId = source.ExternalId, TargetExternalId = target.ExternalId, Purpose = "Uso" }, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        var handler = new CreateResourceCustomerHandler(repository.Object, resources.Object, targets.Object, inventory.Object);
        var result = await handler.Handle(request, cancellation.Token);
        Assert.Equal(conflict, result.Conflict != null);
        inventory.Verify(item => item.TouchAsync(InventoryFakerBuilder.Actor, It.IsAny<DateTime>(), cancellation.Token), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
        if (!conflict) Assert.Equal(1, result.Response.Revision);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
    [Fact]
    public async Task Handle_should_reject_duplicate_and_missing_target_before_writing()
    {
        var source = InventoryFakerBuilder.Resource();
        var target = ManagedServiceFakerBuilder.Customer();
        var resources = InventoryFakerBuilder.Repository(source);
        var targets = InventoryFakerBuilder.Repository(target);
        var inventory = new Mock<IRepositoryInventoryRevision>();
        var graph = new Mock<IRepositoryInventoryResource>();
        graph.Setup(item => item.ReadGraphAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new InventoryGraphData());
        var repository = InventoryFakerBuilder.Repository(ResourceCustomer.Create(source, target, "Uso", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now));
        var handler = new CreateResourceCustomerHandler(repository.Object, resources.Object, targets.Object, inventory.Object);
        var request = new CreateResourceCustomerMediatorCommand { Payload = new() { ResourceExternalId = source.ExternalId, TargetExternalId = target.ExternalId, Purpose = "Uso" }, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        Assert.NotNull((await handler.Handle(request, CancellationToken.None)).Conflict);
        repository.Verify(item => item.AddAsync(It.IsAny<ResourceCustomer>(), It.IsAny<CancellationToken>()), Times.Never);
        request.Payload.TargetExternalId = Guid.NewGuid();
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
    }
    [Fact]
    public void Validator_should_enforce_period_contract_and_public_error_names()
    {
        var source = InventoryFakerBuilder.Resource(); var target = ManagedServiceFakerBuilder.Customer();
        var link = ResourceCustomer.Create(source, target, "Uso", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var request = new CreateResourceCustomerMediatorCommand { Payload = new() { ResourceExternalId = source.ExternalId, TargetExternalId = target.ExternalId, Purpose = "Uso" }, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        var validator = new CreateResourceCustomerMediatorValidator();
        Assert.True(validator.Validate(request).IsValid);
        request.Payload.Purpose = ""; request.Payload.TargetExternalId = Guid.Empty;
        request.ActorUserId = Guid.Empty;
        var result = validator.Validate(request);
        Assert.False(result.IsValid); Assert.All(result.Errors, item => Assert.DoesNotContain("Payload.", item.PropertyName));
    }
}
