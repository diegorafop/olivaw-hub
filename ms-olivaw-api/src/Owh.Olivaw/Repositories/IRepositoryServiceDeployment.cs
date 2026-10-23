using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Repositories;
public interface IRepositoryServiceDeployment : IRepository<ServiceDeployment>
{
    Task<ServiceDeploymentPage> ReadPageAsync(Guid? serviceId, string environment, int page, int size, CancellationToken token);
}
public sealed class ServiceDeploymentPage
{
    public long Total { get; set; }
    public List<ServiceDeployment> Items { get; set; } = new();
}
