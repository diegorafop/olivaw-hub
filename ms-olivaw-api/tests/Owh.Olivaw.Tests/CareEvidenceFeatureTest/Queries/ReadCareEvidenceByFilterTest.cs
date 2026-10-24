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
using Owh.Olivaw.Api.Features.CareEvidenceFeature.Commands;
using Owh.Olivaw.Api.Features.CareEvidenceFeature.Queries;
using Owh.Olivaw.Client.Features.CareEvidenceFeature.Commands;
using Owh.Olivaw.Client.Features.CareEvidenceFeature.Queries;
namespace Owh.Olivaw.Tests.CareEvidenceFeatureTest.Queries;
public sealed class ReadCareEvidenceByFilterTest
{
    [Fact]
    public async Task Handle_should_filter_resource_and_page_without_writes()
    {
        using var cancellation = new CancellationTokenSource();
        var resource = InventoryFakerBuilder.Resource(); var other = InventoryFakerBuilder.Resource(2);
        var readings = Enumerable.Range(1, 25).Select(id => { var item = CareEvidence.Create(resource, Guid.NewGuid(), InventoryFakerBuilder.Now, "Fonte", "Registro", "backup", "success", "https://example.test/evidence", InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now); item.Id = id; return item; }).ToArray();
        var repository = InventoryFakerBuilder.Repository(readings);
        var handler = new ReadCareEvidenceByFilterHandler(repository.Object, InventoryFakerBuilder.Repository(resource, other).Object);
        var request = new ReadCareEvidenceByFilterMediatorQuery { Payload = new() { ResourceExternalId = resource.ExternalId } };
        Assert.True(new ReadCareEvidenceByFilterMediatorValidator().Validate(request).IsValid);
        var first = await handler.Handle(request, cancellation.Token); Assert.Equal(20, first.Items.Count); Assert.Equal(6, first.NextBeforeId);
        request.Payload.BeforeId = first.NextBeforeId!.Value;
        var second = await handler.Handle(request, cancellation.Token); Assert.Equal(5, second.Items.Count); Assert.Null(second.NextBeforeId);
        Assert.All(first.Items, item => Assert.Equal("Fonte", item.Source));
        request.Payload.ResourceExternalId = other.ExternalId; Assert.Empty((await handler.Handle(request, cancellation.Token)).Items);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        request.Payload.BeforeId = -1; Assert.False(new ReadCareEvidenceByFilterValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
}
