using System.Security.Cryptography;
using System.Text;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
using Owh.Olivaw.Api.Features.InventoryFeature.Internals;
namespace Owh.Olivaw.Api.Services;
public sealed class ScheduledHealthOptions
{
    public bool Enabled { get; set; }
    public int IntervalMinutes { get; set; } = 15;
    public int RetentionDays { get; set; } = 30;
    public Guid ActorUserId { get; set; }
    public Guid[] ResourceExternalIds { get; set; } = Array.Empty<Guid>();
    public void Validate()
    {
        if(!Enabled)return;
        if(IntervalMinutes<5 || IntervalMinutes>60 || RetentionDays<7 || RetentionDays>90 || ActorUserId==Guid.Empty || ResourceExternalIds.Length<1 || ResourceExternalIds.Length>32 || ResourceExternalIds.Any(x=>x==Guid.Empty) || ResourceExternalIds.Distinct().Count()!=ResourceExternalIds.Length) throw new InvalidOperationException("Configure coleta agendada: intervalo 5–60 min, retenção 7–90 dias, autor e até 32 recursos únicos.");
    }
}
public sealed class HealthCollectionStatus
{
    private readonly object gate=new();
    private DateTime? completedAt;
    private int attempted;
    private int failures;
    public void Complete(DateTime at,int count,int errors){lock(gate){completedAt=at;attempted=count;failures=errors;}}
    public (DateTime? CompletedAt,int Attempted,int Failures) Read(){lock(gate){return(completedAt,attempted,failures);}}
}
public sealed class ScheduledHealthCycle(IRepository<InventoryResource> resources,IRepository<HealthReading> readings,PublicHealthProbe probe,HealthCollectionStatus status,ILogger<ScheduledHealthCycle> logger)
{
    public const string Source="Verificação HTTP agendada";
    public static Guid SampleKey(Guid resource,DateTime now,int minutes)
    {
        var bucket=now.Ticks/TimeSpan.FromMinutes(minutes).Ticks;
        return new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(resource.ToString("D")+":"+bucket))[..16]);
    }
    public async Task RunAsync(ScheduledHealthOptions options,CancellationToken token)
    {
        options.Validate();if(!options.Enabled)return;var attempted=0;var failures=0;
        foreach(var id in options.ResourceExternalIds)
        {
            token.ThrowIfCancellationRequested();attempted++;
            try
            {
                var resource=await resources.GetFirstOrDefaultAsync(x=>x.ExternalId==id && x.DeletedAt==null,token);
                if(resource==null || !probe.IsAllowed(resource.ReferenceUrl)){failures++;logger.LogWarning("Coleta ignorada: recurso {ResourceId} ausente ou destino não autorizado.",id);continue;}
                var key=SampleKey(id,DateTime.UtcNow,options.IntervalMinutes);
                if(await readings.ExistsAsync(x=>x.InventoryResourceId==resource.Id && x.SampleKey==key,token))continue;
                var outcome=await probe.ReadAsync(resource.ReferenceUrl,token);var now=DateTime.UtcNow;
                if(outcome=="source-error")failures++;
                var reading=HealthReading.Create(resource,key,now,Source,"Somente resposta HTTP; não verifica todos os fluxos de negócio.",outcome,options.IntervalMinutes*2,options.ActorUserId,"Coletor Olivaw",now);
                reading.MarkScheduled();
                await readings.AddAsync(reading,token);
                if(!await InventoryRules.SaveAsync(readings,token)) failures++;
            }
            catch(OperationCanceledException) when(token.IsCancellationRequested){throw;}
            catch(Exception){failures++;logger.LogWarning("Coleta não concluída para recurso {ResourceId}; resultado não apresentado como saudável.",id);}
        }
        // Remover somente amostras desta fonte e destes recursos, preservando entradas manuais.
        var cutoff=DateTime.UtcNow.AddDays(-options.RetentionDays);
        var stale=(await readings.TakeOrderedAsync(x=>x.IsScheduled && x.Source==Source && x.ObservedAt<cutoff && options.ResourceExternalIds.Contains(x.InventoryResource.ExternalId),500,x=>x.Id,token)).ToArray();
        foreach(var reading in stale)await readings.RemoveAsync(reading,token);
        if(stale.Length>0)await readings.SaveChangesAsync(token);
        status.Complete(DateTime.UtcNow,attempted,failures);
    }
}
public sealed class ScheduledHealthCollector(IServiceScopeFactory scopes,ScheduledHealthOptions options,ILogger<ScheduledHealthCollector> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        options.Validate();if(!options.Enabled)return;
        // Sem sobreposição: a próxima janela começa após concluir o ciclo atual.
        while(!token.IsCancellationRequested)
        {
            try{using var scope=scopes.CreateScope();await scope.ServiceProvider.GetRequiredService<ScheduledHealthCycle>().RunAsync(options,token);}
            catch(OperationCanceledException) when(token.IsCancellationRequested){break;}
            catch(Exception){logger.LogWarning("Ciclo de coleta interrompido; aguarda próxima janela.");}
            await Task.Delay(TimeSpan.FromMinutes(options.IntervalMinutes),token);
        }
    }
}
