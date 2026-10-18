using System.Threading;
using System.Threading.Tasks;

namespace Mps.Devops.Services;

public interface IDiskCleanupService
{
    Task<DeploymentStatus> CreateAsync(CancellationToken cancellationToken);
}
