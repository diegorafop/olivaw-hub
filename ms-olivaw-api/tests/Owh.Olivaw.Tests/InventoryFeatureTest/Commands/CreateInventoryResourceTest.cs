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
public sealed class CreateInventoryResourceTest
{
    [Fact]
    public async Task Handle_should_create_snapshot_and_propagate_token_and_cancel()
    {
        using var cancellation = new CancellationTokenSource();
        var repository = InventoryFakerBuilder.Repository<InventoryResource>();
        var request = Request();
        var result = await new CreateInventoryResourceHandler(repository.Object).Handle(request, cancellation.Token);
        Assert.Null(result.Conflict); Assert.NotEqual(Guid.Empty, result.Response.ExternalId); Assert.Equal(1, result.Response.Revision);
        repository.Verify(item => item.AddAsync(It.Is<InventoryResource>(resource => resource.Changes.Count == 1 && resource.MemoryCapacityGiB == null && resource.ActorUserId == request.ActorUserId), cancellation.Token), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
        repository.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<InventoryResource, bool>>>(), cancellation.Token)).ThrowsAsync(new OperationCanceledException(cancellation.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => new CreateInventoryResourceHandler(repository.Object).Handle(request, cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(() => new CreateInventoryResourceHandler(repository.Object).Handle(null!, cancellation.Token));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_should_reject_duplicate_identity_or_write_race(bool race)
    {
        var repository = InventoryFakerBuilder.Repository<InventoryResource>();
        if (race) repository.Setup(item => item.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateConcurrencyException());
        else repository.Setup(item => item.ExistsAsync(It.IsAny<Expression<Func<InventoryResource, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Assert.NotNull((await new CreateInventoryResourceHandler(repository.Object).Handle(Request(), CancellationToken.None)).Conflict);
    }
    [Theory]
    [InlineData("identity")]
    [InlineData("type")]
    [InlineData("capacity")]
    [InlineData("precision")]
    [InlineData("source")]
    [InlineData("url")]
    [InlineData("monitoring")]
    public void Validator_should_reject_invalid_resource(string field)
    {
        var request = Request(); var validator = new CreateInventoryResourceMediatorValidator();
        Assert.True(validator.Validate(request).IsValid);
        switch(field) {
            case "identity": request.Payload.TechnicalId = "Bad_ID"; break;
            case "type": request.Payload.Type = "application"; break;
            case "capacity": request.Payload.MemoryCapacityGiB = -1; break;
            case "precision": request.Payload.CpuCapacity = 0.0001M; break;
            case "source": request.Payload.Status = "verified"; break;
            case "url": request.Payload.ReferenceUrl = "https://example.test?token=secret"; break;
            case "monitoring": request.Payload.Monitoring = "healthy"; break;
        }
        var result = validator.Validate(request); Assert.False(result.IsValid); Assert.All(result.Errors, item => Assert.DoesNotContain("Payload.", item.PropertyName));
    }
    internal static CreateInventoryResourceMediatorCommand Request() => new() { ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador", Payload = new() { TechnicalId = "vm-test", Name = "VM teste", Type = "compute" } };
}
