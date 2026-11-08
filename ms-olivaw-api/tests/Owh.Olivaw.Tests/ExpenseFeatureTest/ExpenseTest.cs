using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Owh.Olivaw.Api.Features.ExpenseFeature.Commands;
using Owh.Olivaw.Api.Features.ExpenseFeature.Queries;
using Owh.Olivaw.Client.Features.ExpenseFeature.Commands;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Tests.Mocks;
using Xunit;
namespace Owh.Olivaw.Tests.ExpenseFeatureTest;
public sealed class ExpenseTest
{
    private static readonly DateTime Day = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static Expense Entry(decimal amount,string kind,Customer? customer=null) => Expense.Create(Guid.NewGuid(),new string('A',64),"AWS","infrastructure",kind,"USD",amount,5.5m,Day,"Fatura",customer,InventoryFakerBuilder.Actor,Day.AddDays(1));
    [Fact] public async Task Query_should_convert_separate_credits_filter_customer_and_report_unallocated_without_double_counting()
    {
        var customer=ManagedServiceFakerBuilder.Customer();var other=ManagedServiceFakerBuilder.Customer(2);
        var repository=InventoryFakerBuilder.Repository(Entry(10,"cost"),Entry(2,"cost",customer),Entry(1,"credit",customer),Entry(3,"cost",other));
        var handler=new ReadExpenseByFilterHandler(repository.Object);
        var request=new ReadExpenseByFilterMediatorQuery{Payload=new(){From=Day,To=Day.AddDays(2),GroupBy="day",IncludeCredits=true}};
        var total=await handler.Handle(request,CancellationToken.None);
        Assert.Equal(82.5m,total.GrossBrl);Assert.Equal(5.5m,total.CreditsBrl);Assert.Equal(77m,total.NetBrl);Assert.Equal(55m,total.UnallocatedBrl);
        Assert.Single(total.ByCategory); Assert.Equal(total.GrossBrl, total.ByCategory.Single().GrossBrl);
        Assert.Equal(3, total.ByCustomer.Count); Assert.Equal(total.NetBrl, total.ByCustomer.Sum(x => x.NetBrl));
        Assert.Equal(55m, total.ByCustomer.Single(x => x.Key == "shared").NetBrl);
        request.Payload.PageSize = 1;
        var paged = await handler.Handle(request,CancellationToken.None);
        Assert.Single(paged.Items); Assert.Equal(total.GrossBrl,paged.GrossBrl); Assert.Equal(3,paged.ByCustomer.Count);
        request.Payload.PageSize = 20;
        Assert.Equal(2,total.Series.Count);Assert.Equal(0,total.Series[1].NetBrl);
        request.Payload.IncludeCredits = false;
        var expensesOnly = await handler.Handle(request,CancellationToken.None);
        Assert.Equal(3, expensesOnly.Total); Assert.Equal(0m,expensesOnly.CreditsBrl); Assert.Equal(82.5m,expensesOnly.NetBrl);
        Assert.All(expensesOnly.Items,x=>Assert.Equal("cost",x.Kind));
        request.Payload.IncludeCredits = true;
        request.Payload.CustomerExternalId=customer.ExternalId;
        var own=await handler.Handle(request,CancellationToken.None);
        Assert.Equal(5.5m,own.NetBrl);Assert.Equal(55m,own.UnallocatedBrl);Assert.Equal(2,own.Items.Count);Assert.All(own.Items,x=>Assert.Equal(customer.Business,x.CustomerName));
        Assert.True(new ReadExpenseByFilterValidator().Validate(request.Payload).IsValid);
        request.Payload.CustomerExternalId = null; request.Payload.UnallocatedOnly = true;
        var shared = await handler.Handle(request,CancellationToken.None);
        Assert.Equal(55m,shared.GrossBrl); Assert.Single(shared.ByCustomer); Assert.Equal("shared",shared.ByCustomer.Single().Key);
        request.Payload.CustomerExternalId=customer.ExternalId; Assert.False(new ReadExpenseByFilterValidator().Validate(request.Payload).IsValid);
        request.Payload.CustomerExternalId=null;
        request.Payload.To=Day.AddDays(367);Assert.False(new ReadExpenseByFilterValidator().Validate(request.Payload).IsValid);
    }
    [Theory][InlineData("BRL",2)][InlineData("USD",0)][InlineData("EUR",1)]
    public void Currency_and_conversion_should_be_explicit(string currency,decimal rate)
    {
        var p=new CreateExpenseCommand{EntryKey=Guid.NewGuid(),Description="Custo",Amount=1,Currency=currency,ExchangeRateToBrl=rate,Source="Fatura",OccurredAt=Day};
        Assert.False(new CreateExpenseValidator().Validate(p).IsValid);
        Assert.Throws<ArgumentException>(()=>Expense.Create(p.EntryKey,new string('A',64),p.Description,p.Category,p.Kind,currency,p.Amount,rate,Day,p.Source,null,InventoryFakerBuilder.Actor,Day.AddDays(1)));
    }
    [Fact]public async Task Create_should_reuse_same_key_and_reject_changed_payload()
    {
        var p=new CreateExpenseCommand{EntryKey=Guid.NewGuid(),Description="Ferramenta",Amount=100,Currency="USD",ExchangeRateToBrl=5.5m,Source="Fatura",OccurredAt=Day};
        var digest=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(p)));
        var saved=Expense.Create(p.EntryKey,digest,p.Description,p.Category,p.Kind,p.Currency,p.Amount,p.ExchangeRateToBrl,Day,p.Source,null,InventoryFakerBuilder.Actor,Day.AddDays(1));
        var repository=InventoryFakerBuilder.Repository(saved);
        var handler=new CreateExpenseHandler(repository.Object,InventoryFakerBuilder.Repository<Customer>().Object);
        var command=new CreateExpenseMediatorCommand{Payload=p,ActorUserId=InventoryFakerBuilder.Actor};
        Assert.Equal(saved.ExternalId,(await handler.Handle(command,CancellationToken.None)).Response.ExternalId);
        p.Amount=101;Assert.NotNull((await handler.Handle(command,CancellationToken.None)).Conflict);
        Assert.True(new CreateExpenseMediatorValidator().Validate(command).IsValid);
    }
    [Fact]
    public void Known_future_expenses_allow_one_year_but_reject_further_dates_and_non_utc_days()
    {
        var now = DateTime.UtcNow;
        var p = new CreateExpenseCommand { EntryKey = Guid.NewGuid(), Description = "Domínio anual — parcela", Amount = 3.33m, Currency = "BRL", ExchangeRateToBrl = 1, Source = "Apropriação mensal de custo anual", OccurredAt = now.Date.AddMonths(11) };
        Assert.True(new CreateExpenseValidator().Validate(p).IsValid);
        Assert.Equal(p.OccurredAt, Expense.Create(p.EntryKey, new string('A',64), p.Description, p.Category, p.Kind, p.Currency, p.Amount, 1, p.OccurredAt, p.Source, null, InventoryFakerBuilder.Actor, now).OccurredAt);
        p.OccurredAt = now.Date.AddYears(1).AddDays(1);
        Assert.False(new CreateExpenseValidator().Validate(p).IsValid);
        Assert.Throws<ArgumentException>(() => Expense.Create(p.EntryKey, new string('A',64), p.Description, p.Category, p.Kind, p.Currency, p.Amount, 1, p.OccurredAt, p.Source, null, InventoryFakerBuilder.Actor, now));
        p.OccurredAt = DateTime.SpecifyKind(now.Date, DateTimeKind.Local);
        Assert.False(new CreateExpenseValidator().Validate(p).IsValid);
    }
}
