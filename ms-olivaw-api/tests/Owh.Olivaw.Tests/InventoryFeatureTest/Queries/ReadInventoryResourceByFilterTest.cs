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
public sealed class ReadInventoryResourceByFilterTest
{
    [Fact]
    public async Task Handle_should_normalize_optional_filters_propagate_token_and_map_shared_resources()
    {
        using var cancellation = new CancellationTokenSource();
        var repository = new Mock<IRepositoryInventoryResource>();
        repository.Setup(item => item.ReadPageAsync(It.Is<InventoryResourceFilter>(filter => filter.Search == "" && filter.Status == "" && filter.Page == 2 && filter.Shared == true), cancellation.Token)).ReturnsAsync(new InventoryResourcePage { Total = 21, Items = new() { new() { Resource = InventoryFakerBuilder.Resource(), Context = new() { CustomerCount = 2, OriginCount = 1, Origins = new() { new() { Kind = "dependency", FromResourceName = "Banco" } } } } } });
        var handler = new ReadInventoryResourceByFilterHandler(repository.Object);
        var request = new ReadInventoryResourceByFilterMediatorQuery { Payload = new() { Page = 2, Search = null, Status = null, Shared = true } };
        var page = await handler.Handle(request, cancellation.Token);
        Assert.Equal(21, page.Total); Assert.Equal(2, Assert.Single(page.Items).CustomerCount); Assert.Equal("Banco", Assert.Single(page.Items[0].Origins).FromResourceName);
        repository.VerifyAll();
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
        Assert.True(new ReadInventoryResourceByFilterMediatorValidator().Validate(request).IsValid);
        request.Payload.Environment = "invalid"; request.Payload.Page = 0;
        Assert.False(new ReadInventoryResourceByFilterValidator().Validate(request.Payload).IsValid);
        repository.Setup(item => item.ReadPageAsync(It.IsAny<InventoryResourceFilter>(), cancellation.Token)).ReturnsAsync(new InventoryResourcePage { LimitExceeded = true });
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, cancellation.Token));
    }
}
