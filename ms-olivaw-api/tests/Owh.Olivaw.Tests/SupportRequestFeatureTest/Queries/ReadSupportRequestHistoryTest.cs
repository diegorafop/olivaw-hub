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
namespace Owh.Olivaw.Tests.SupportRequestFeatureTest.Queries;
public sealed class ReadSupportRequestHistoryTest
{
    [Fact]
    public async Task Handle_should_read_scoped_history_or_records_and_validate_filter()
    {
        var anchor = ManagedServiceFakerBuilder.Customer(); var entity = SupportRequest.Create(anchor, new() { Title = "Solicitação", Impact = "Impacto", Owner = "Operador", NextAction = "Avaliar", InternalNote = "Nota inicial", Category = "question", Requester = "Pessoa" }, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now, null);
        var repository = InventoryFakerBuilder.Repository(entity);
        var handler = new ReadSupportRequestHistoryHandler(repository.Object, InventoryFakerBuilder.Repository(entity.Changes.ToArray()).Object);
        var request = new ReadSupportRequestHistoryMediatorQuery { Payload = new() { ExternalId = entity.ExternalId } };
        Assert.True(new ReadSupportRequestHistoryMediatorValidator().Validate(request).IsValid);
        var result = await handler.Handle(request, CancellationToken.None); Assert.Single(result.Items); Assert.Equal("Operador", result.Items.Single().ActorName);
        request.Payload.ExternalId = Guid.Empty; Assert.False(new ReadSupportRequestHistoryValidator().Validate(request.Payload).IsValid);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
