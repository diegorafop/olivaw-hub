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
namespace Owh.Olivaw.Tests.UsageReadingFeatureTest.Commands;
public sealed class CreateUsageReadingTest
{
    [Fact]
    public async Task Handle_should_preserve_source_token_and_sample_idempotency()
    {
        using var cancellation = new CancellationTokenSource();
        var resource = InventoryFakerBuilder.Resource();
        var resources = InventoryFakerBuilder.Repository(resource);
        var repository = InventoryFakerBuilder.Repository<UsageReading>();
        var handler = new CreateUsageReadingHandler(repository.Object, resources.Object);
        var payload = new CreateUsageReadingCommand { ResourceExternalId = resource.ExternalId, SampleKey = Guid.NewGuid(), ObservedAt = InventoryFakerBuilder.Now, Source = "Conferência de teste", Note = "Registro", Metric = "storage-gib", Value = 2, Quality = "manual" };
        var request = new CreateUsageReadingMediatorCommand { Payload = payload, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        Assert.True(new CreateUsageReadingMediatorValidator().Validate(request).IsValid);
        var first = await handler.Handle(request, cancellation.Token);
        var retry = await handler.Handle(request, cancellation.Token);
        Assert.Equal(first.Response.ExternalId, retry.Response.ExternalId);
        repository.Verify(item => item.AddAsync(It.Is<UsageReading>(value => value.SampleKey == payload.SampleKey && value.Source == payload.Source && value.ObservedAt == payload.ObservedAt), cancellation.Token), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
        payload.Source = "Outra fonte";
        Assert.NotNull((await handler.Handle(request, cancellation.Token)).Conflict);
        payload.SampleKey = Guid.NewGuid();
        payload.ResourceExternalId = Guid.NewGuid();
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, cancellation.Token));
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
    [Fact]
    public void Validator_should_reject_future_time_missing_source_and_invalid_classification()
    {
        var payload = new CreateUsageReadingCommand { ResourceExternalId = Guid.NewGuid(), SampleKey = Guid.NewGuid(), ObservedAt = InventoryFakerBuilder.Now, Source = "Fonte", Metric = "storage-gib", Value = 2, Quality = "manual" };
        var validator = new CreateUsageReadingValidator(); Assert.True(validator.Validate(payload).IsValid);
        payload.ObservedAt = DateTime.UtcNow.AddDays(1); Assert.False(validator.Validate(payload).IsValid);
        payload.ObservedAt = InventoryFakerBuilder.Now; payload.Source = ""; Assert.False(validator.Validate(payload).IsValid);
        payload.Source = "Fonte";
        payload.Value = -1;
        Assert.False(validator.Validate(payload).IsValid);
    }
}
