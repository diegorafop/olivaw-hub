using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class FinancialAccount : Entity
{
    public Guid AccountKey {get;private set;}
    public string Name {get;private set;}=string.Empty;
    public decimal OpeningBalance {get;private set;}
    public DateTime OpenedAt {get;private set;}
    private FinancialAccount(){}
    public static FinancialAccount Create(Guid key,string name,decimal opening,DateTime date,Guid actor,DateTime now)
    {
        if(key==Guid.Empty || string.IsNullOrWhiteSpace(name) || name.Length>120 || decimal.Round(opening,2)!=opening || Math.Abs(opening)>100000000 || date.Kind!=DateTimeKind.Utc || date.TimeOfDay!=TimeSpan.Zero || date>now || actor==Guid.Empty)throw new ArgumentException("Conta financeira inválida.");
        return new(){ExternalId=Guid.NewGuid(),AccountKey=key,Name=name,OpeningBalance=opening,OpenedAt=date,ActorUserId=actor,CreatedAt=now};
    }
}
