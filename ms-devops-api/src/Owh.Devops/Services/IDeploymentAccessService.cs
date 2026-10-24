namespace Owh.Devops.Services;

public interface IDeploymentAccessService
{
    void EnsureCanDeploy(string folder);
}
