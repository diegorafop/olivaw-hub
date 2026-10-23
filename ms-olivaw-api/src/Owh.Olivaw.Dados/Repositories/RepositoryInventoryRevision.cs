using Microsoft.EntityFrameworkCore;
using Owh.Olivaw.Domain;
using Owh.Olivaw.Repositories;
namespace Owh.Olivaw.Dados.Repositories;

public sealed class RepositoryInventoryRevision(WriteDbContext context) : Repository<InventoryRevision>(context), IRepositoryInventoryRevision
{
    private bool _touched;
    public async Task TouchAsync(Guid actor, DateTime now, CancellationToken token)
    {
        if (_touched) return;
        var gate = await _context.Set<InventoryRevision>().FirstOrDefaultAsync(item => item.Key == "topology", token);
        if (gate == null) await _context.Set<InventoryRevision>().AddAsync(InventoryRevision.Create(actor, now), token);
        else gate.Touch(actor, now);
        _touched = true;
    }
    public async Task CloseServiceDeploymentsAsync(int serviceId, Guid actor, string actorName, DateTime now, CancellationToken token)
    {
        await TouchAsync(actor, now, token);
        var deployments = await _context.Set<ServiceDeployment>().Where(item => item.ManagedServiceId == serviceId && item.EndedAt == null && item.DeletedAt == null).ToListAsync(token);
        foreach (var deployment in deployments)
        {
            var change = deployment.Apply(deployment.Name, deployment.Location, deployment.Endpoint, "closed", actor, actorName, now);
            await _context.Set<ServiceDeploymentChange>().AddAsync(change, token);
        }
        var ids = deployments.Select(item => item.Id).ToArray();
        var uses = await _context.Set<DeploymentResource>().Where(item => ids.Contains(item.ServiceDeploymentId) && item.EndedAt == null && item.DeletedAt == null).ToListAsync(token);
        foreach (var use in uses) use.Close(actor, actorName, now);
    }
}
