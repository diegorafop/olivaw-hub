using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Api.Features.PlanFeature.Commands;
using Owh.Olivaw.Client.Features.PlanFeature.Commands;
using Xunit;
namespace Owh.Olivaw.Tests.PlanFeatureTest.Commands;
public sealed class CreatePlanCostSheetTest
{
 [Fact]public async Task Handle_should_include_annual_domain_shared_tools_labor_reserve_and_commercial_provision_atomically()
 {
  var studies=InventoryFakerBuilder.Repository<PlanStudy>();var costs=InventoryFakerBuilder.Repository<CostReference>();var h=new CreatePlanCostSheetHandler(studies.Object,costs.Object);
  var q=new CreatePlanCostSheetMediatorCommand{ActorUserId=InventoryFakerBuilder.Actor,ActorName="Operador",Payload=new(){ScenarioKey=Guid.NewGuid(),PlanKey="bronze",Source="Bases hipotéticas de teste",IncludedFeatures="Site estático",SupportHours=0.1m,HourlyCost=80,ReservePercent=15,MarginPercent=20,CommercialPercent=6,Lines=CreatePlanCostSheetHandler.Keys.Select(key=>new PlanCostSheetLine{Key=key,Amount=key=="domain"?40:key=="vm"?100:key=="tools"?100:0,Period=key=="domain"?"annual":"monthly",SharedClients=10}).ToList()}};
  Assert.True(new CreatePlanCostSheetMediatorValidator().Validate(q).IsValid);var first=await h.Handle(q,CancellationToken.None);Assert.Null(first.Conflict);
  Assert.Equal(32.58m,first.Response.MonthlyCostWithReserve);Assert.Equal(44.03m,first.Response.SuggestedMonthlyPrice);
  costs.Verify(x=>x.AddAsync(It.IsAny<CostReference>(),It.IsAny<CancellationToken>()),Times.Exactly(8));studies.Verify(x=>x.SaveChangesAsync(It.IsAny<CancellationToken>()),Times.Once);costs.Verify(x=>x.SaveChangesAsync(It.IsAny<CancellationToken>()),Times.Never);
  Assert.Equal(first.Response.ExternalId,(await h.Handle(q,CancellationToken.None)).Response.ExternalId);
  q.Payload.HourlyCost=90;Assert.NotNull((await h.Handle(q,CancellationToken.None)).Conflict);
  q.Payload.CommercialPercent=80;Assert.False(new CreatePlanCostSheetValidator().Validate(q.Payload).IsValid);
  await Assert.ThrowsAsync<ArgumentNullException>(()=>h.Handle(null!,CancellationToken.None));
 }
 [Fact]public void Validator_should_reject_duplicate_missing_unknown_lines_and_incomplete_basis()
 {
  var p=new CreatePlanCostSheetCommand{ScenarioKey=Guid.NewGuid(),PlanKey="bronze",Source="Teste",IncludedFeatures="Site",Lines=CreatePlanCostSheetHandler.Keys.Select(k=>new PlanCostSheetLine{Key=k,Amount=0,SharedClients=1}).ToList()};
  Assert.True(new CreatePlanCostSheetValidator().Validate(p).IsValid);p.Lines[0].SharedClients=0;Assert.False(new CreatePlanCostSheetValidator().Validate(p).IsValid);p.Lines[0].SharedClients=1;p.Lines[0].Key="storage";Assert.False(new CreatePlanCostSheetValidator().Validate(p).IsValid);
 }
}
