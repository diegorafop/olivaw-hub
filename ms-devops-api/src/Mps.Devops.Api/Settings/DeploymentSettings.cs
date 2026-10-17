namespace Mps.Devops.Api.Settings;

public sealed class DeploymentSettings
{
    public string RootDirectory { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 300;
}
