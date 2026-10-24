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
namespace Owh.Olivaw.Tests.HealthReadingFeatureTest.Commands;
public sealed class CreateHealthReadingTest
{
    [Fact]
    public async Task Handle_should_preserve_source_token_and_sample_idempotency()
    {
        using var cancellation = new CancellationTokenSource();
        var resource = InventoryFakerBuilder.Resource();
        var resources = InventoryFakerBuilder.Repository(resource);
        var repository = InventoryFakerBuilder.Repository<HealthReading>();
        var handler = new CreateHealthReadingHandler(repository.Object, resources.Object);
        var payload = new CreateHealthReadingCommand { ResourceExternalId = resource.ExternalId, SampleKey = Guid.NewGuid(), ObservedAt = InventoryFakerBuilder.Now, Source = "Conferência de teste", Note = "Registro", Outcome = "success", MaxAgeMinutes = 15 };
        var request = new CreateHealthReadingMediatorCommand { Payload = payload, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        Assert.True(new CreateHealthReadingMediatorValidator().Validate(request).IsValid);
        var first = await handler.Handle(request, cancellation.Token);
        var retry = await handler.Handle(request, cancellation.Token);
        Assert.Equal(first.Response.ExternalId, retry.Response.ExternalId);
        repository.Verify(item => item.AddAsync(It.Is<HealthReading>(value => value.SampleKey == payload.SampleKey && value.Source == payload.Source && value.ObservedAt == payload.ObservedAt), cancellation.Token), Times.Once);
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
        var payload = new CreateHealthReadingCommand { ResourceExternalId = Guid.NewGuid(), SampleKey = Guid.NewGuid(), ObservedAt = InventoryFakerBuilder.Now, Source = "Fonte", Outcome = "success", MaxAgeMinutes = 15 };
        var validator = new CreateHealthReadingValidator(); Assert.True(validator.Validate(payload).IsValid);
        payload.ObservedAt = DateTime.UtcNow.AddDays(1); Assert.False(validator.Validate(payload).IsValid);
        payload.ObservedAt = InventoryFakerBuilder.Now; payload.Source = ""; Assert.False(validator.Validate(payload).IsValid);
        payload.Source = "Fonte";
        payload.Outcome = "invented";
        Assert.False(validator.Validate(payload).IsValid);
    }
}
