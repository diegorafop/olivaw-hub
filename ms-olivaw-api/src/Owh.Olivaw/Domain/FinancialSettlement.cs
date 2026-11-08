using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class FinancialSettlement : Entity
{
    public Guid SettlementKey {get;private set;}
    public int FinancialEntryId {get;private set;}
    public FinancialEntry FinancialEntry {get;private set;}=null!;
    public int FinancialAccountId {get;private set;}
    public FinancialAccount FinancialAccount {get;private set;}=null!;
    public decimal Amount {get;private set;}
    public bool IsRefund {get;private set;}
    public DateTime PaidAt {get;private set;}
    public string Source {get;private set;}=string.Empty;
    private FinancialSettlement(){}
    public static FinancialSettlement Create(Guid key,FinancialEntry entry,FinancialAccount account,decimal amount,bool refund,DateTime date,string source,Guid actor,DateTime now)
    {
        if(key==Guid.Empty || entry==null || account==null || amount<=0 || amount>100000000 || decimal.Round(amount,2)!=amount || date.Kind!=DateTimeKind.Utc || date.TimeOfDay!=TimeSpan.Zero || date>now || date<account.OpenedAt || string.IsNullOrWhiteSpace(source) || source.Length>300 || actor==Guid.Empty)throw new ArgumentException("Comprovante de baixa inválido.");
        return new(){ExternalId=Guid.NewGuid(),SettlementKey=key,FinancialEntry=entry,FinancialEntryId=entry.Id,FinancialAccount=account,FinancialAccountId=account.Id,Amount=amount,IsRefund=refund,PaidAt=date,Source=source,ActorUserId=actor,CreatedAt=now};
    }
}
public sealed class FinancialTransfer : Entity
{
    public Guid TransferKey {get;private set;}
    public int FromAccountId {get;private set;}
    public FinancialAccount FromAccount {get;private set;}=null!;
    public int ToAccountId {get;private set;}
    public FinancialAccount ToAccount {get;private set;}=null!;
    public decimal Amount {get;private set;}
    public DateTime TransferredAt {get;private set;}
    public string Source {get;private set;}=string.Empty;
    private FinancialTransfer(){}
    public static FinancialTransfer Create(Guid key,FinancialAccount from,FinancialAccount to,decimal amount,DateTime date,string source,Guid actor,DateTime now)
    {
        if(key==Guid.Empty || from==null || to==null || from.Id==to.Id || amount<=0 || amount>100000000 || decimal.Round(amount,2)!=amount || date.Kind!=DateTimeKind.Utc || date.TimeOfDay!=TimeSpan.Zero || date>now || date<from.OpenedAt || date<to.OpenedAt || string.IsNullOrWhiteSpace(source) || source.Length>300 || actor==Guid.Empty)throw new ArgumentException("Transferência inválida.");
        return new(){ExternalId=Guid.NewGuid(),TransferKey=key,FromAccount=from,FromAccountId=from.Id,ToAccount=to,ToAccountId=to.Id,Amount=amount,TransferredAt=date,Source=source,ActorUserId=actor,CreatedAt=now};
    }
}
