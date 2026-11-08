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
public sealed class ReadCustomerPlanByFilterTest
{
    [Fact]
    public async Task Handle_should_read_scoped_immutable_versions_without_writes()
    {
        var cost = CostReference.Create("VM", 100, "Teste", InventoryFakerBuilder.Now, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var customer = ManagedServiceFakerBuilder.Customer(); var entity = CustomerPlan.Create(customer, "Atual", 50, null, 2, "Condições", "Aceite", InventoryFakerBuilder.Now, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        entity.Id = 1;
        var repository = InventoryFakerBuilder.Repository(entity);
        var handler = new ReadCustomerPlanByFilterHandler(repository.Object); var request = new ReadCustomerPlanByFilterMediatorQuery { Payload = new() { CustomerExternalId = customer.ExternalId, At = InventoryFakerBuilder.Now } };
        Assert.True(new ReadCustomerPlanByFilterMediatorValidator().Validate(request).IsValid);
        var response = await handler.Handle(request, CancellationToken.None); Assert.Single(response.Items); Assert.Equal(50, response.Current!.MonthlyPrice);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        request.Payload.BeforeId = -1; Assert.False(new ReadCustomerPlanByFilterValidator().Validate(request.Payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
