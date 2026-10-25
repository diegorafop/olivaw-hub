using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Api.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Client.Features.ManagedServiceFeature.Queries;
using Owh.Olivaw.Repositories;
using Xunit;

namespace Owh.Olivaw.Tests.ManagedServiceFeatureTest.Queries;
public class ReadManagedServiceByFilterTest
{
    [Fact]
    public async Task Handle_should_forward_filters_and_pagination_and_map_shared_count()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        using var cancellation = new CancellationTokenSource();
        var repository = new Mock<IRepositoryManagedService>();
        repository.Setup(item => item.ReadPageAsync("dotnet", "active", "dotnet", id, 2, 10, cancellation.Token))
            .ReturnsAsync(new ManagedServiceReadPage { Total = 11, Items = new() { new() { ExternalId = id, Name = "API", TechnicalId = "ms-example-api", Type = "dotnet", Scope = "customer", Status = "active", CustomerCount = 2, CreatedAt = createdAt } } });
        var handler = new ReadManagedServiceByFilterHandler(repository.Object);
        var result = await handler.Handle(new() { Payload = new() { Search = "dotnet", Status = "active", Type = "dotnet", CustomerExternalId = id, Page = 2, PageSize = 10 } }, cancellation.Token);
        Assert.Equal(11, result.Total);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, Assert.Single(result.Items).CustomerCount);
        Assert.Equal("ms-example-api", result.Items[0].TechnicalId);
        Assert.Equal(createdAt, result.Items[0].CreatedAt);
        repository.VerifyAll();
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
    [Fact]
    public void Validator_should_reject_invalid_filters_and_unbounded_page()
    {
        var validator = new ReadManagedServiceByFilterValidator();
        Assert.True(validator.Validate(new ReadManagedServiceByFilterQuery()).IsValid);
        Assert.False(validator.Validate(new ReadManagedServiceByFilterQuery { PageSize = 101 }).IsValid);
        Assert.False(validator.Validate(new ReadManagedServiceByFilterQuery { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new ReadManagedServiceByFilterQuery { Status = "healthy" }).IsValid);
        Assert.False(validator.Validate(new ReadManagedServiceByFilterQuery { Type = "disk" }).IsValid);
        Assert.False(validator.Validate(new ReadManagedServiceByFilterQuery { CustomerExternalId = Guid.Empty }).IsValid);
    }
}
