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
using Owh.Olivaw.Api.Features.UsageReadingFeature.Commands;
using Owh.Olivaw.Api.Features.UsageReadingFeature.Queries;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Commands;
using Owh.Olivaw.Client.Features.UsageReadingFeature.Queries;
namespace Owh.Olivaw.Tests.UsageReadingFeatureTest.Queries;
public sealed class ReadUsageReadingByFilterTest
{
    [Fact]
    public async Task Handle_should_filter_resource_and_page_without_writes()
    {
        using var cancellation = new CancellationTokenSource();
        var resource = InventoryFakerBuilder.Resource(); var other = InventoryFakerBuilder.Resource(2);
        var readings = Enumerable.Range(1, 25).Select(id => { var item = UsageReading.Create(resource, Guid.NewGuid(), InventoryFakerBuilder.Now, "Fonte", "Registro", "storage-gib", 2, "manual", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now); item.Id = id; return item; }).ToArray();
        var repository = InventoryFakerBuilder.Repository(readings);
        var handler = new ReadUsageReadingByFilterHandler(repository.Object, InventoryFakerBuilder.Repository(resource, other).Object);
        var request = new ReadUsageReadingByFilterMediatorQuery { Payload = new() { ResourceExternalId = resource.ExternalId } };
        Assert.True(new ReadUsageReadingByFilterMediatorValidator().Validate(request).IsValid);
        var first = await handler.Handle(request, cancellation.Token); Assert.Equal(20, first.Items.Count); Assert.Equal(6, first.NextBeforeId);
        request.Payload.BeforeId = first.NextBeforeId!.Value;
        var second = await handler.Handle(request, cancellation.Token); Assert.Equal(5, second.Items.Count); Assert.Null(second.NextBeforeId);
        Assert.All(first.Items, item => Assert.Equal("Fonte", item.Source));
        request.Payload.ResourceExternalId = other.ExternalId; Assert.Empty((await handler.Handle(request, cancellation.Token)).Items);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        request.Payload.BeforeId = -1; Assert.False(new ReadUsageReadingByFilterValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
}
