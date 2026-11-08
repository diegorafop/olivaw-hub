using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentValidation;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.OlivawCore.Repositories;
using Owh.Olivaw.Api.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Api.Features.HealthReadingFeature.Queries;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Commands;
using Owh.Olivaw.Client.Features.HealthReadingFeature.Queries;
namespace Owh.Olivaw.Tests.HealthReadingFeatureTest.Queries;
public sealed class ReadHealthReadingByFilterTest
{
    [Fact]
    public async Task Handle_should_filter_resource_and_page_without_writes()
    {
        using var cancellation = new CancellationTokenSource();
        var resource = InventoryFakerBuilder.Resource(); var other = InventoryFakerBuilder.Resource(2);
        var readings = Enumerable.Range(1, 25).Select(id => { var item = HealthReading.Create(resource, Guid.NewGuid(), InventoryFakerBuilder.Now, "Fonte", "Registro", "success", 15, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now); item.Id = id; return item; }).ToArray();
        var repository = InventoryFakerBuilder.Repository(readings);
        var handler = new ReadHealthReadingByFilterHandler(repository.Object, InventoryFakerBuilder.Repository(resource, other).Object);
        var request = new ReadHealthReadingByFilterMediatorQuery { Payload = new() { ResourceExternalId = resource.ExternalId } };
        Assert.True(new ReadHealthReadingByFilterMediatorValidator().Validate(request).IsValid);
        var first = await handler.Handle(request, cancellation.Token); Assert.Equal(20, first.Items.Count); Assert.Equal(6, first.NextBeforeId);
        request.Payload.BeforeId = first.NextBeforeId!.Value;
        var second = await handler.Handle(request, cancellation.Token); Assert.Equal(5, second.Items.Count); Assert.Null(second.NextBeforeId);
        Assert.All(first.Items, item => Assert.Equal("Fonte", item.Source));
        request.Payload.ResourceExternalId = other.ExternalId; Assert.Empty((await handler.Handle(request, cancellation.Token)).Items);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        request.Payload.BeforeId = -1; Assert.False(new ReadHealthReadingByFilterValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
}
