using System;
using Owh.OlivawCore.Domain;
namespace Owh.Olivaw.Domain;
public sealed class PlanVersion : Entity
{
    public Guid PublicationKey { get; private set; }
    public string Digest { get; private set; } = string.Empty;
    public string PlanKey { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public decimal MonthlyPrice { get; private set; }
    public decimal? StorageLimitGiB { get; private set; }
    public decimal? MemoryLimitMiB { get; private set; }
    public decimal? CpuLimit { get; private set; }
    public string Conditions { get; private set; } = string.Empty;
    public string ApprovalReference { get; private set; } = string.Empty;
    public DateTime EffectiveAt { get; private set; }
    public int? PlanStudyId { get; private set; }
    public PlanStudy? PlanStudy { get; private set; }
    private PlanVersion() { }
    public static bool IsPlan(string key) => key is "bronze" or "prata" or "ouro" or "diamante";
    public static PlanVersion Create(Guid key,string digest,string planKey,int version,decimal price,decimal? storage,decimal? memory,decimal? cpu,string conditions,string approval,DateTime effectiveAt,PlanStudy? study,Guid actor,DateTime now)
    {
        if(key==Guid.Empty || actor==Guid.Empty || digest.Length!=64 || !IsPlan(planKey) || version<1) throw new ArgumentException("Identificação de versão inválida.");
        if(price<0 || price>100000000 || decimal.Round(price,2)!=price) throw new ArgumentException("Preço inválido.");
        if(storage<=0 || storage>1000000000 || memory<=0 || memory>1000000000 || cpu<=0 || cpu>1024) throw new ArgumentException("Capacidade inválida.");
        foreach(var v in new[]{storage,memory,cpu}) if(v.HasValue && decimal.Round(v.Value,3)!=v) throw new ArgumentException("Capacidade admite três casas decimais.");
        if(string.IsNullOrWhiteSpace(conditions) || conditions.Length>3000 || string.IsNullOrWhiteSpace(approval) || approval.Length>300) throw new ArgumentException("Condições e aprovação obrigatórias.");
        if(effectiveAt.Kind!=DateTimeKind.Utc || effectiveAt.Day!=1 || effectiveAt.TimeOfDay!=TimeSpan.Zero) throw new ArgumentException("Vigência deve iniciar em competência mensal UTC.");
        return new(){ExternalId=Guid.NewGuid(),PublicationKey=key,Digest=digest,PlanKey=planKey,Version=version,MonthlyPrice=price,StorageLimitGiB=storage,MemoryLimitMiB=memory,CpuLimit=cpu,Conditions=conditions,ApprovalReference=approval,EffectiveAt=effectiveAt,PlanStudy=study,PlanStudyId=study?.Id,ActorUserId=actor,CreatedAt=now};
    }
}
