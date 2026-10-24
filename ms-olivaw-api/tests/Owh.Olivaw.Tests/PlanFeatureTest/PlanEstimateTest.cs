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
namespace Owh.Olivaw.Tests.PlanFeatureTest;
public sealed class PlanEstimateTest
{
    [Fact]
    public void Calculate_should_separate_allocated_cost_support_reserve_and_sales_margin()
    {
        var result = PlanEstimate.Calculate(new[] { new PlanCostLine { Name = "VM de teste", MonthlyCost = 100, AllocationPercent = 20 } }, 1, 30, 10, 20);
        Assert.Equal(20, result.KnownMonthlyCost); Assert.Equal(80, result.UnallocatedMonthlyCost); Assert.Equal(68.75M, result.SuggestedMonthlyPrice); Assert.Equal(0, result.MissingCosts);
    }
    [Fact]
    public void Calculate_should_leave_unknown_price_pending_instead_of_treating_it_as_zero()
    {
        var result = PlanEstimate.Calculate(new[] { new PlanCostLine { Name = "Disco", MonthlyCost = null, AllocationPercent = 25 } }, 0, 0, 0, 0);
        Assert.Null(result.SuggestedMonthlyPrice); Assert.Equal(1, result.MissingCosts);
        Assert.Null(PlanEstimate.Calculate(Array.Empty<PlanCostLine>(), 0, 0, 0, 0).SuggestedMonthlyPrice);
    }
    [Theory]
    [InlineData(100)] [InlineData(-1)] [InlineData(99.999)]
    public void Calculate_should_reject_invalid_margin_without_overflow(decimal margin)
    {
        Assert.Throws<ArgumentException>(() => PlanEstimate.Calculate(new[] { new PlanCostLine { Name = "VM", MonthlyCost = 100, AllocationPercent = 20 } }, 0, 0, 0, margin));
    }
    [Fact]
    public void Create_should_preserve_study_limits_features_and_cost_version_without_contract_changes()
    {
        var cost = CostReference.Create("VM", 100, "Valor fictício de teste", InventoryFakerBuilder.Now, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        var study = PlanStudy.Create("Bronze — estudo", "Site simples e manutenção", 2, null, new[] { (cost, 20M) }, 0, 0, 0, 20, InventoryFakerBuilder.Actor, "Operador", InventoryFakerBuilder.Now);
        Assert.Equal(2, study.StorageLimitGB); Assert.Null(study.MemoryLimitMiB); Assert.Equal(25, study.SuggestedMonthlyPrice);
        Assert.Same(cost, study.Costs.Single().CostReference); Assert.Equal("Site simples e manutenção", study.IncludedFeatures);
    }
}
