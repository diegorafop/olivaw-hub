using System;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Repositories;

// Coordena versão de topologia e encerramentos na unidade de persistência compartilhada.
public interface IRepositoryInventoryRevision : IRepository<InventoryRevision>
{
    Task TouchAsync(Guid actor, DateTime now, CancellationToken token);
    Task CloseServiceDeploymentsAsync(int serviceId, Guid actor, string actorName, DateTime now, CancellationToken token);
}
