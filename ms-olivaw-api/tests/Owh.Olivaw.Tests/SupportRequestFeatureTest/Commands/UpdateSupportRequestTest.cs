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
public sealed class UpdateSupportRequestTest
{
    [Fact]
    public async Task Handle_should_preserve_history_and_reject_stale_revision()
    {
        var anchor = ManagedServiceFakerBuilder.Customer(); var data = new SupportRequestData { Title = "Solicitação", Impact = "Impacto", Owner = "Operador", NextAction = "Avaliar", InternalNote = "Nota inicial", Category = "question", Requester = "Pessoa" };
        var entity = SupportRequest.Create(anchor, data, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now, null);
        var repository = InventoryFakerBuilder.Repository(entity); var changes = InventoryFakerBuilder.Repository<SupportRequestChange>();
        var handler = new UpdateSupportRequestHandler(repository.Object, changes.Object);
        var request = new UpdateSupportRequestMediatorCommand { ExternalId = entity.ExternalId, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Outro operador", Payload = new() { Revision = 1, Title = "Solicitação", Impact = "Impacto", Owner = "Operador", NextAction = "Avaliar", InternalNote = "Nota inicial", Category = "question", Requester = "Pessoa" } };
        request.Payload.NextAction = "Próximo passo";
        Assert.True(new UpdateSupportRequestMediatorValidator().Validate(request).IsValid);
        var result = await handler.Handle(request, CancellationToken.None); Assert.Null(result.Conflict); Assert.Equal(2, result.Response.Revision);
        Assert.Equal("Avaliar", entity.Changes.Single().NextAction);
        changes.Verify(item => item.AddAsync(It.Is<SupportRequestChange>(value => value.Revision == 2 && value.NextAction == "Próximo passo" && value.ActorName == "Outro operador"), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull((await handler.Handle(request, CancellationToken.None)).Conflict);
        request.Payload.Status = "invalid"; Assert.False(new UpdateSupportRequestValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
