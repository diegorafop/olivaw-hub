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
using Owh.Olivaw.Api.Features.SupportRequestFeature.Commands;
using Owh.Olivaw.Api.Features.SupportRequestFeature.Queries;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Commands;
using Owh.Olivaw.Client.Features.SupportRequestFeature.Queries;
namespace Owh.Olivaw.Tests.SupportRequestFeatureTest.Commands;
public sealed class CreateSupportRequestTest
{
    [Fact]
    public async Task Handle_should_create_audited_initial_state_and_reject_missing_anchor()
    {
        using var cancellation = new CancellationTokenSource();
        var anchor = ManagedServiceFakerBuilder.Customer(); var anchors = InventoryFakerBuilder.Repository(anchor); var repository = InventoryFakerBuilder.Repository<SupportRequest>();
        var handler = new CreateSupportRequestHandler(repository.Object, anchors.Object, InventoryFakerBuilder.Repository<ManagedService>().Object, InventoryFakerBuilder.Repository<ManagedServiceCustomer>().Object);
        var request = new CreateSupportRequestMediatorCommand { ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador", Payload = new() { CustomerExternalId = anchor.ExternalId, Title = "Solicitação", Impact = "Impacto", Owner = "Operador", NextAction = "Avaliar", InternalNote = "Nota inicial", Category = "question", Requester = "Pessoa" } };
        Assert.True(new CreateSupportRequestMediatorValidator().Validate(request).IsValid);
        var result = await handler.Handle(request, cancellation.Token); Assert.Null(result.Conflict); Assert.Equal(1, result.Response.Revision);
        repository.Verify(item => item.AddAsync(It.Is<SupportRequest>(value => value.Status == "received" && value.Changes.Single().ActorName == "Operador" && value.CustomerId == anchor.Id), cancellation.Token), Times.Once);
        request.Payload.CustomerExternalId = Guid.NewGuid(); await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, cancellation.Token));
        request.Payload.Status = "closed"; Assert.False(new CreateSupportRequestValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
}
