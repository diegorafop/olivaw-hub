namespace Mps.Devops.Api.Features.ImplantacaoFeature.Internals;

public sealed class DeploymentGate : IDisposable
{
    public SemaphoreSlim Semaphore { get; } = new(1, 1);
    public void Dispose() => Semaphore.Dispose();
}
