using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Owh.Olivaw.Api.Features.FinancialFeature.Queries;
using Xunit;
namespace Owh.Olivaw.Tests.FinancialFeatureTest.Queries;
public sealed class ReadFinancialSummaryTest
{
 [Fact]public async Task Handle_should_distinguish_obligations_actual_cash_and_refunds()
 {
  var a=FinancialFakerBuilder.Account();var due=FinancialFakerBuilder.Entry();var revenue=FinancialFakerBuilder.Entry(2,"receivable");var day=FinancialFakerBuilder.Day;
  var payment=FinancialSettlement.Create(System.Guid.NewGuid(),due,a,20,false,day,"Comprovante",InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);
  var receipt=FinancialSettlement.Create(System.Guid.NewGuid(),revenue,a,50,false,day,"Comprovante",InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);
  var refund=FinancialSettlement.Create(System.Guid.NewGuid(),due,a,5,true,day.AddDays(1),"Estorno",InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);
  var h=new ReadFinancialSummaryHandler(InventoryFakerBuilder.Repository(a).Object,InventoryFakerBuilder.Repository(due,revenue).Object,InventoryFakerBuilder.Repository(payment,receipt,refund).Object);
  var q=new ReadFinancialSummaryMediatorQuery{Payload=new(){From=day,To=day.AddMonths(1)}};Assert.True(new ReadFinancialSummaryValidator().Validate(q.Payload).IsValid);
  var result=await h.Handle(q,CancellationToken.None);Assert.Equal(15,result.PaidBrl);Assert.Equal(50,result.ReceivedBrl);Assert.Equal(35,result.PayableBrl);Assert.Equal(0,result.ReceivableBrl);Assert.Equal(135,result.ClosingBalanceBrl);Assert.Equal(31,result.Series.Count);
 }
}
