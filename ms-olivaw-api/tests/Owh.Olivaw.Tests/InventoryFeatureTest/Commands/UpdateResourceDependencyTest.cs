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
public sealed class UpdateResourceDependencyTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_should_preserve_periods_and_report_concurrent_changes(bool conflict)
    {
        using var cancellation = new CancellationTokenSource();
        var source = InventoryFakerBuilder.Resource();
        var target = InventoryFakerBuilder.Resource(2);
        var resources = InventoryFakerBuilder.Repository(source);
        var targets = InventoryFakerBuilder.Repository(target);
        var inventory = new Mock<IRepositoryInventoryRevision>();
        var graph = new Mock<IRepositoryInventoryResource>();
        graph.Setup(item => item.ReadGraphAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new InventoryGraphData());
        var link = ResourceDependency.Create(source, target, "Uso", "depends-on", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var repository = InventoryFakerBuilder.Repository<ResourceDependency>(link);
        if (conflict) repository.Setup(item => item.SaveChangesAsync(cancellation.Token)).ThrowsAsync(new DbUpdateConcurrencyException());
        var request = new UpdateResourceDependencyMediatorCommand { ExternalId = link.ExternalId, Payload = new() { Revision = 1 }, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        var handler = new UpdateResourceDependencyHandler(repository.Object, inventory.Object);
        var result = await handler.Handle(request, cancellation.Token);
        Assert.Equal(conflict, result.Conflict != null);
        inventory.Verify(item => item.TouchAsync(InventoryFakerBuilder.Actor, It.IsAny<DateTime>(), cancellation.Token), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
        Assert.NotNull(link.EndedAt); Assert.Equal("Operador", link.EndedByName); Assert.Equal(2, link.Revision);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
    [Fact]
    public async Task Handle_should_reject_stale_revision_without_saving()
    {
        var source = InventoryFakerBuilder.Resource();
        var target = InventoryFakerBuilder.Resource(2);
        var resources = InventoryFakerBuilder.Repository(source);
        var targets = InventoryFakerBuilder.Repository(target);
        var inventory = new Mock<IRepositoryInventoryRevision>();
        var graph = new Mock<IRepositoryInventoryResource>();
        graph.Setup(item => item.ReadGraphAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new InventoryGraphData());
        var link = ResourceDependency.Create(source, target, "Uso", "depends-on", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var repository = InventoryFakerBuilder.Repository(link);
        var request = new UpdateResourceDependencyMediatorCommand { ExternalId = link.ExternalId, Payload = new() {Revision = 99}, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        Assert.NotNull((await new UpdateResourceDependencyHandler(repository.Object, inventory.Object).Handle(request, CancellationToken.None)).Conflict);
        Assert.Null(link.EndedAt);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public void Validator_should_enforce_period_contract_and_public_error_names()
    {
        var source = InventoryFakerBuilder.Resource(); var target = InventoryFakerBuilder.Resource(2);
        var link = ResourceDependency.Create(source, target, "Uso", "depends-on", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var request = new UpdateResourceDependencyMediatorCommand { ExternalId = link.ExternalId, Payload = new() { Revision = 1 }, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        var validator = new UpdateResourceDependencyMediatorValidator();
        Assert.True(validator.Validate(request).IsValid);
        request.Payload.Revision = 0; request.ExternalId = Guid.Empty;
        request.ActorUserId = Guid.Empty;
        var result = validator.Validate(request);
        Assert.False(result.IsValid); Assert.All(result.Errors, item => Assert.DoesNotContain("Payload.", item.PropertyName));
    }
}
