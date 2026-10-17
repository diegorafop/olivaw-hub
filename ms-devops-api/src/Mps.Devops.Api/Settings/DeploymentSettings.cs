namespace Mps.Devops.Api.Settings;

public sealed class DeploymentSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string User { get; set; } = "ec2-user";
    public string KeyFile { get; set; } = "/run/secrets/deployment_key";
    public string KnownHostsFile { get; set; } = "/run/secrets/deployment_known_hosts";
    public int TimeoutSeconds { get; set; } = 30;
}
