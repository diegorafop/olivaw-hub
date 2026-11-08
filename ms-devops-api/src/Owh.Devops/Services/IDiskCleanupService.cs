using System.Threading;
using System.Threading.Tasks;

namespace Owh.Devops.Services;

public interface IDiskCleanupService
{
    Task<DeploymentStatus> CreateAsync(CancellationToken cancellationToken);
}
