using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Owh.Olivaw.Domain;
using Owh.OlivawCore.Repositories;
namespace Owh.Olivaw.Repositories;
// Latest observation per resource requires correlated SQL/grouping absent from the generic contract.
public interface IRepositoryHealthReading : IRepository<HealthReading>
{
    Task<IReadOnlyList<HealthReading>> ReadLatestAsync(int[] resourceIds, CancellationToken token);
}
