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
using Owh.Olivaw.Api.Features.PlanFeature.Commands;
using Owh.Olivaw.Api.Features.PlanFeature.Queries;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Queries;
namespace Owh.Olivaw.Tests.PlanFeatureTest.Queries;
public sealed class ReadPlanStudyByFilterTest
{
    [Fact]
    public async Task Handle_should_read_scoped_immutable_versions_without_writes()
    {
        var cost = CostReference.Create("VM", 100, "Teste", InventoryFakerBuilder.Now, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var customer = ManagedServiceFakerBuilder.Customer(); var entity = PlanStudy.Create("Bronze", "Site simples", 2, null, new[] { (cost, 20M) }, 0, 0, 0, 20, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        entity.Id = 0;
        var repository = InventoryFakerBuilder.Repository(entity);
        var handler = new ReadPlanStudyByFilterHandler(repository.Object, InventoryFakerBuilder.Repository(entity.Costs.ToArray()).Object); var request = new ReadPlanStudyByFilterMediatorQuery();
        Assert.True(new ReadPlanStudyByFilterMediatorValidator().Validate(request).IsValid);
        var response = await handler.Handle(request, CancellationToken.None); Assert.Single(response.Items); Assert.Equal("VM", response.Items.Single().Costs.Single().Name); Assert.Equal(2, response.Items.Single().StorageLimitGB);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        request.Payload.BeforeId = -1; Assert.False(new ReadPlanStudyByFilterValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
