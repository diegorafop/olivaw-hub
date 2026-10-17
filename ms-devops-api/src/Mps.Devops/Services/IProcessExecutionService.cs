using System.Threading;
using System.Threading.Tasks;

namespace Mps.Devops.Services;

public enum DeploymentStep { Checkout, Pull, Compose }

public sealed class ProcessExecutionResult
{
    public bool Success { get; set; }
    public string Log { get; set; } = string.Empty;
}

public interface IProcessExecutionService
{
    Task<ProcessExecutionResult> ExecuteAsync(string folder, string branch, string environment, DeploymentStep step, CancellationToken cancellationToken);
}
