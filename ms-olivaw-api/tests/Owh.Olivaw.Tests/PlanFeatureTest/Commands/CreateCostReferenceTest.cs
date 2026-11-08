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
namespace Owh.Olivaw.Tests.PlanFeatureTest.Commands;
public sealed class CreateCostReferenceTest
{
    [Fact]
    public async Task Handle_should_validate_payload_and_preserve_audit_and_token()
    {
        using var cancellation = new CancellationTokenSource();
        var customer = ManagedServiceFakerBuilder.Customer(); var customers = InventoryFakerBuilder.Repository(customer);
        var cost = CostReference.Create("VM", 100, "Teste", InventoryFakerBuilder.Now, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now); var costs = InventoryFakerBuilder.Repository(cost);
        var repository = InventoryFakerBuilder.Repository<CostReference>();
        var handler = new CreateCostReferenceHandler(repository.Object);
        var payload = new CreateCostReferenceCommand { Name = "VM", MonthlyCost = 100, Source = "Teste", EffectiveAt = InventoryFakerBuilder.Now };
        var request = new CreateCostReferenceMediatorCommand { Payload = payload, ActorUserId = InventoryFakerBuilder.Actor, ActorName = "Operador" };
        Assert.True(new CreateCostReferenceMediatorValidator().Validate(request).IsValid);
        var result = await handler.Handle(request, cancellation.Token); Assert.Null(result.Conflict); Assert.NotEqual(Guid.Empty, result.Response.ExternalId);
        repository.Verify(item => item.AddAsync(It.Is<CostReference>(value => value.ActorUserId == InventoryFakerBuilder.Actor), cancellation.Token), Times.Once);
        repository.Verify(item => item.SaveChangesAsync(cancellation.Token), Times.Once);
        payload.Name = ""; Assert.False(new CreateCostReferenceValidator().Validate(payload).IsValid);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, cancellation.Token));
    }
}
