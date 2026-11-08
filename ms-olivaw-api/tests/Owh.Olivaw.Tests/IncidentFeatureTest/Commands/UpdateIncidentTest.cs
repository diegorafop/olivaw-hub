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
public sealed class UpdateIncidentTest
{
    [Fact]
    public async Task Handle_should_preserve_history_and_reject_stale_revision()
    {
        var anchor = InventoryFakerBuilder.Resource(); var data = new IncidentData { Title = "Solicitação", Impact = "Impacto", Owner = "Operador", NextAction = "Avaliar", InternalNote = "Nota inicial", Severity = "major" };
        var entity = Incident.Create(anchor, data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var repository = InventoryFakerBuilder.Repository(entity); var changes = InventoryFakerBuilder.Repository<IncidentChange>();
        var handler = new UpdateIncidentHandler(repository.Object, changes.Object);
        var request = new UpdateIncidentMediatorCommand { ExternalId = entity.ExternalId, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Outro operador", Payload = new() { Revision = 1, Title = "Solicitação", Impact = "Impacto", Owner = "Operador", NextAction = "Avaliar", InternalNote = "Nota inicial", Severity = "major" } };
        request.Payload.NextAction = "Próximo passo";
        Assert.True(new UpdateIncidentMediatorValidator().Validate(request).IsValid);
        var result = await handler.Handle(request, CancellationToken.None); Assert.Null(result.Conflict); Assert.Equal(2, result.Response.Revision);
        Assert.Equal("Avaliar", entity.Changes.Single().NextAction);
        changes.Verify(item => item.AddAsync(It.Is<IncidentChange>(value => value.Revision == 2 && value.NextAction == "Próximo passo" && value.ActorName == "Outro operador"), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull((await handler.Handle(request, CancellationToken.None)).Conflict);
        request.Payload.Status = "invalid"; Assert.False(new UpdateIncidentValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
