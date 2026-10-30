using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class FinancialEntry : Entity
{
    public Guid EntryKey {get;private set;}
    public string Description {get;private set;}=string.Empty;
    public string Direction {get;private set;}=string.Empty;
    public string Category {get;private set;}=string.Empty;
    public decimal Amount {get;private set;}
    public decimal SettledAmount {get;private set;}
    public DateTime DueAt {get;private set;}
    public string Source {get;private set;}=string.Empty;
    public string Status {get;private set;}="open";
    public string CancellationReason {get;private set;}=string.Empty;
    public long Revision {get;private set;}=1;
    public int? CustomerId {get;private set;}
    public Customer? Customer {get;private set;}
    public int? ExpenseId {get;private set;}
    public Expense? Expense {get;private set;}
    private FinancialEntry(){}
    public static FinancialEntry Create(Guid key,string description,string direction,string category,decimal amount,DateTime due,string source,Customer? customer,Expense? expense,Guid actor,DateTime now)
    {
        if(key==Guid.Empty || actor==Guid.Empty || string.IsNullOrWhiteSpace(description) || description.Length>160 || string.IsNullOrWhiteSpace(source) || source.Length>300 || direction is not("payable" or "receivable") || category is not("infrastructure" or "tools" or "labor" or "tax" or "subscription" or "other") || amount<=0 || amount>100000000 || decimal.Round(amount,2)!=amount || due.Kind!=DateTimeKind.Utc || due.TimeOfDay!=TimeSpan.Zero || due.Year<2000 || due.Year>2100)throw new ArgumentException("Obrigação financeira inválida.");
        if(expense!=null && (direction!="payable" || expense.Kind!="cost"))throw new ArgumentException("Somente despesa de custo pode originar conta a pagar.");
        return new(){ExternalId=Guid.NewGuid(),EntryKey=key,Description=description,Direction=direction,Category=category,Amount=amount,DueAt=due,Source=source,Customer=customer,CustomerId=customer?.Id,Expense=expense,ExpenseId=expense?.Id,ActorUserId=actor,CreatedAt=now};
    }
    public void Settle(decimal amount,bool refund,Guid actor,DateTime now)
    {
        if(Status=="cancelled" || amount<=0 || decimal.Round(amount,2)!=amount || actor==Guid.Empty)throw new ArgumentException("Baixa inválida.");
        var result=SettledAmount+(refund?-amount:amount);if(result<0 || result>Amount)throw new ArgumentException("Baixa excede saldo da obrigação.");
        SettledAmount=result;Status=result==Amount?"settled":result==0?"open":"partial";ActorUserId=actor;UpdatedAt=now;Revision++;
    }
    public void Cancel(string reason,Guid actor,DateTime now)
    {
        if(SettledAmount!=0 || string.IsNullOrWhiteSpace(reason) || reason.Length>500 || actor==Guid.Empty)throw new ArgumentException("Estorne as baixas antes de cancelar e informe motivo.");
        if(Status=="cancelled")return;Status="cancelled";CancellationReason=reason;ActorUserId=actor;UpdatedAt=now;Revision++;
    }
}
