using System.Threading;
using System.Threading.Tasks;

namespace Mps.Devops.Services;

public sealed class ProcessExecutionResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
}

public interface IProcessExecutionService
{
    Task<ProcessExecutionResult> ExecuteAsync(string payload, CancellationToken cancellationToken);
}
