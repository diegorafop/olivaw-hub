using System;
using Owh.Olivaw.Domain;
namespace Owh.Olivaw.Tests.Mocks;
internal static class FinancialFakerBuilder
{
 internal static readonly DateTime Day=new(2026,10,1,0,0,0,DateTimeKind.Utc);
 internal static FinancialAccount Account(int id=1){var e=FinancialAccount.Create(Guid.NewGuid(),"Conta "+id,100,Day,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);e.Id=id;return e;}
 internal static FinancialEntry Entry(int id=1,string direction="payable",Customer? customer=null){var e=FinancialEntry.Create(Guid.NewGuid(),"Obrigação "+id,direction,"other",50,Day,"Contrato teste",customer,null,InventoryFakerBuilder.Actor,InventoryFakerBuilder.Now);e.Id=id;return e;}
}
