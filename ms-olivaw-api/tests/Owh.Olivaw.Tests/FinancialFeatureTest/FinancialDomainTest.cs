using System;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.FinancialFeatureTest;
public sealed class FinancialDomainTest
{
 [Fact]public void Entry_should_preserve_partial_refund_and_cancellation_history_without_overpayment()
 {
  var e=FinancialFakerBuilder.Entry();e.Settle(20,false,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);Assert.Equal("partial",e.Status);Assert.Equal(20,e.SettledAmount);
  Assert.Throws<ArgumentException>(()=>e.Settle(31,false,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now));Assert.Throws<ArgumentException>(()=>e.Cancel("Sem contrato",InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now));
  e.Settle(30,false,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);Assert.Equal("settled",e.Status);e.Settle(50,true,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);Assert.Equal("open",e.Status);Assert.Equal(0,e.SettledAmount);
  Assert.Throws<ArgumentException>(()=>e.Settle(1,true,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now));e.Cancel("Cancelamento aprovado",InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);Assert.Equal("cancelled",e.Status);Assert.Equal("Cancelamento aprovado",e.CancellationReason);Assert.Throws<ArgumentException>(()=>e.Settle(1,false,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now));
 }
 [Fact]public void Money_should_reject_future_payment_incompatible_account_and_fractional_cents()
 {
  var a=FinancialFakerBuilder.Account();var e=FinancialFakerBuilder.Entry();Assert.Throws<ArgumentException>(()=>FinancialSettlement.Create(Guid.NewGuid(),e,a,1,false,DateTime.UtcNow.Date.AddDays(1),"Teste",InventoryFakerBuilder.Actor,DateTime.UtcNow));
  Assert.Throws<ArgumentException>(()=>FinancialSettlement.Create(Guid.NewGuid(),e,a,0.001m,false,FinancialFakerBuilder.Day,"Teste",InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now));
  Assert.Throws<ArgumentException>(()=>FinancialTransfer.Create(Guid.NewGuid(),a,a,1,FinancialFakerBuilder.Day,"Teste",InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now));
 }
}
