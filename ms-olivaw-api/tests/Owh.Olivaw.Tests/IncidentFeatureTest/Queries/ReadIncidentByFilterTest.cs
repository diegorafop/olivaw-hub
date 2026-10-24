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
namespace Owh.Olivaw.Tests.IncidentFeatureTest.Queries;
public sealed class ReadIncidentByFilterTest
{
    [Fact]
    public async Task Handle_should_read_scoped_history_or_records_and_validate_filter()
    {
        var anchor = InventoryFakerBuilder.Resource(); var entity = Incident.Create(anchor, new() { Title = "Solicitação", Impact = "Impacto", Owner = "Operador", NextAction = "Avaliar", InternalNote = "Nota inicial", Severity = "major" }, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var repository = InventoryFakerBuilder.Repository(entity);
        var handler = new ReadIncidentByFilterHandler(repository.Object);
        var request = new ReadIncidentByFilterMediatorQuery { Payload = new() { ResourceExternalId = anchor.ExternalId } };
        Assert.True(new ReadIncidentByFilterMediatorValidator().Validate(request).IsValid);
        var result = await handler.Handle(request, CancellationToken.None); Assert.Single(result.Items); Assert.Equal(entity.Title, result.Items.Single().Title);
        request.Payload.Status = "invalid"; Assert.False(new ReadIncidentByFilterValidator().Validate(request.Payload).IsValid);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
