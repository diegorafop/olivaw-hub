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
public sealed class ReadPlanEstimateTest
{
    [Fact]
    public async Task Handle_should_use_persisted_prices_and_reject_duplicate_origin_versions()
    {
        var cost = CostReference.Create("VM", 100, "Teste", InventoryFakerBuilder.Now, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var previous = CostReference.Create("VM", 80, "Anterior", InventoryFakerBuilder.Now.AddDays(-1), InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var repository = InventoryFakerBuilder.Repository(cost, previous); var handler = new ReadPlanEstimateHandler(repository.Object);
        var request = new ReadPlanEstimateMediatorQuery { Payload = new() { Lines = new() { new() { CostReferenceExternalId = cost.ExternalId, AllocationPercent = 20 } }, SupportHours = 1, HourlyCost = 30, ReservePercent = 10, MarginPercent = 20 } };
        Assert.True(new ReadPlanEstimateMediatorValidator().Validate(request).IsValid);
        var response = await handler.Handle(request, CancellationToken.None); Assert.Equal(68.75M, response.SuggestedMonthlyPrice); Assert.Equal(20, response.KnownMonthlyCost);
        request.Payload.Lines.Add(new() { CostReferenceExternalId = previous.ExternalId, AllocationPercent = 20 });
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(request, CancellationToken.None));
        request.Payload.MarginPercent = 100; Assert.False(new ReadPlanEstimateValidator().Validate(request.Payload).IsValid);
        request.Payload.MarginPercent = 10; request.Payload.Lines.Add(null!); Assert.False(new ReadPlanEstimateValidator().Validate(request.Payload).IsValid);
        repository.Verify(item => item.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null!, CancellationToken.None));
    }
}
