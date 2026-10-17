using System.Linq;
using FluentValidation;
using Mps.Devops.Api.Services;
using Mps.Devops.Api.Settings;
using Xunit;

namespace Mps.Devops.Tests.ServicesTest;

public class ProcessExecutionServiceTest
{
    [Fact]
    public void CreateStartInfo_should_use_fixed_remote_command_and_strict_host_checking()
    {
        var start = ProcessExecutionService.CreateStartInfo(new DeploymentSettings { Host = "aws.internal" });
        Assert.Equal("ssh", start.FileName); Assert.False(start.UseShellExecute);
        Assert.True(start.RedirectStandardInput); Assert.True(start.RedirectStandardOutput);
        Assert.True(start.RedirectStandardError);
        Assert.Contains("StrictHostKeyChecking=yes", start.ArgumentList);
        Assert.Contains("BatchMode=yes", start.ArgumentList);
        Assert.Contains("IdentitiesOnly=yes", start.ArgumentList);
        Assert.Contains("ClearAllForwardings=yes", start.ArgumentList);
        Assert.Contains("IdentityAgent=none", start.ArgumentList);
        Assert.Equal("olivaw-deploy", start.ArgumentList.Last());
        Assert.Equal(string.Empty, start.Arguments);
        Assert.DoesNotContain("git", start.ArgumentList); Assert.DoesNotContain("docker", start.ArgumentList);
    }

    [Theory]
    [InlineData("")] [InlineData("-oProxyCommand=id")] [InlineData("host;id")] [InlineData("$(id)")]
    public void CreateStartInfo_should_reject_unsafe_host(string host)
    {
        Assert.Throws<ValidationException>(() => ProcessExecutionService.CreateStartInfo(new DeploymentSettings { Host = host }));
    }

    [Fact]
    public void CreateStartInfo_should_reject_unsafe_user_key_path_and_port()
    {
        Assert.Throws<ValidationException>(() => ProcessExecutionService.CreateStartInfo(new DeploymentSettings { Host = "aws", User = "root;id" }));
        Assert.Throws<ValidationException>(() => ProcessExecutionService.CreateStartInfo(new DeploymentSettings { Host = "aws", KnownHostsFile = "/tmp/%h" }));
        Assert.Throws<ValidationException>(() => ProcessExecutionService.CreateStartInfo(new DeploymentSettings { Host = "aws", Port = 0 }));
    }
}
