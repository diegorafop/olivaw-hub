using System;
using System.Threading;
using System.Threading.Tasks;

namespace Owh.Devops.Services;

public sealed class DeploymentRequest
{
    public string Folder { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}

public sealed class DeploymentStatus
{
    public string Folder { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Log { get; set; } = string.Empty;
}

public interface IDeploymentService
{
    Task<DeploymentStatus> SubmitAsync(DeploymentRequest request, CancellationToken cancellationToken);
    Task<DeploymentStatus> GetAsync(Guid id, CancellationToken cancellationToken);
}
