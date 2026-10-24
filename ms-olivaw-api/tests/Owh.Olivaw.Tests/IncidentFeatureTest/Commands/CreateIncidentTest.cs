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
using Owh.Olivaw.Api.Features.IncidentFeature.Commands;
using Owh.Olivaw.Api.Features.IncidentFeature.Queries;
using Owh.Olivaw.Client.Features.IncidentFeature.Commands;
using Owh.Olivaw.Client.Features.IncidentFeature.Queries;
namespace Owh.Olivaw.Tests.IncidentFeatureTest.Commands;
public sealed class CreateIncidentTest
{
    [Fact]
    public async Task Handle_should_create_audited_initial_state_and_reject_missing_anchor()
    {
        using var cancellation = new CancellationTokenSource();
        var anchor = InventoryFakerBuilder.Resource(); var anchors = InventoryFakerBuilder.Repository(anchor); var repository = InventoryFakerBuilder.Repository<Incident>();
        var handler = new CreateIncidentHandler(repository.Object, anchors.Object);
        var request = new CreateIncidentMediatorCommand { ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador", Payload = new() { ResourceExternalId = anchor.ExternalId, Title = "Solicitação", Impact = "Impacto", Owner = "Operador", NextAction = "Avaliar", InternalNote = "Nota inicial", Severity = "major" } };
        Assert.True(new CreateIncidentMediatorValidator().Validate(request).IsValid);
        var result = await handler.Handle(request, cancellation.Token); Assert.Null(result.Conflict); Assert.Equal(1, result.Response.Revision);
        repository.Verify(item => item.AddAsync(It.Is<Incident>(value => value.Status == "detected" && value.Changes.Single().ActorName == "Operador" && value.ResourceId == anchor.Id), cancellation.Token), Times.Once);
        request.Payload.ResourceExternalId = Guid.NewGuid(); await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, cancellation.Token));
        request.Payload.Status = "closed"; Assert.False(new CreateIncidentValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
}
