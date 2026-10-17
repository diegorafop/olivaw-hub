using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Options;
using Mps.Devops.Api.Services;
using Mps.Devops.Api.Settings;
using Mps.Devops.Services;
using Xunit;

namespace Mps.Devops.Tests.ServicesTest;

public class ProcessExecutionServiceTest
{
    [Theory]
    [InlineData(DeploymentStep.Checkout, "git", "checkout|feature/test")]
    [InlineData(DeploymentStep.Pull, "git", "pull|--ff-only")]
    [InlineData(DeploymentStep.Compose, "docker", "compose|--env-file|docker/.env.dev|-f|docker/docker-compose.yml|-f|docker/docker-compose.dev.yml|up|-d")]
    public void CreateStartInfo_should_use_argument_list_without_shell(DeploymentStep step, string executable, string arguments)
    {
        var start = ProcessExecutionService.CreateStartInfo("/deploy/repo", "feature/test", "dev", step);
        Assert.False(start.UseShellExecute);
        Assert.Equal("/deploy/repo", start.WorkingDirectory);
        Assert.Equal(executable, start.FileName);
        Assert.Equal(arguments.Split('|'), start.ArgumentList.ToArray());
        Assert.Equal(string.Empty, start.Arguments);
        Assert.True(start.RedirectStandardOutput);
        Assert.True(start.RedirectStandardError);
    }

    [Theory]
    [InlineData("../outside", "main")] [InlineData("repo;id", "main")]
    [InlineData("repo", "main&&id")] [InlineData("repo", "--help")]
    public async Task ExecuteAsync_should_reject_injection_before_launch(string folder, string branch)
    {
        var service = new ProcessExecutionService(Options.Create(new DeploymentSettings()));
        await Assert.ThrowsAsync<ValidationException>(() => service.ExecuteAsync(folder, branch, "dev", DeploymentStep.Checkout, CancellationToken.None));
    }

    [Fact]
    public void ResolveDirectory_should_reject_missing_or_unconfigured_root()
    {
        var service = new ProcessExecutionService(Options.Create(new DeploymentSettings()));
        Assert.Throws<ValidationException>(() => service.ResolveDirectory("repo"));
    }

    [Fact]
    public void ResolveDirectory_should_accept_repository_and_reject_symlink_and_non_repository()
    {
        var root = Path.Combine(Path.GetTempPath(), "olivaw-deployment-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "repo", ".git"));
        Directory.CreateDirectory(Path.Combine(root, "empty"));
        try
        {
            var service = new ProcessExecutionService(Options.Create(new DeploymentSettings { RootDirectory = root }));
            Assert.Equal(Path.Combine(root, "repo"), service.ResolveDirectory("repo"));
            Directory.CreateDirectory(Path.Combine(root, "repo", "docker"));
            Assert.Equal(Path.Combine(root, "repo", "docker"), service.ResolveDirectory("repo/docker"));
            Assert.Throws<ValidationException>(() => service.ResolveDirectory("empty"));
            Assert.Throws<ValidationException>(() => service.ResolveDirectory("missing"));
            Directory.CreateSymbolicLink(Path.Combine(root, "alias"), Path.Combine(root, "repo"));
            Assert.Throws<ValidationException>(() => service.ResolveDirectory("alias"));
            Directory.Delete(Path.Combine(root, "repo", ".git"));
            Directory.CreateSymbolicLink(Path.Combine(root, "repo", ".git"), Path.Combine(root, "empty"));
            Assert.Throws<ValidationException>(() => service.ResolveDirectory("repo"));
        }
        finally { Directory.Delete(root, true); }
    }
    [Theory]
    [InlineData("dev")] [InlineData("qas")] [InlineData("stg")] [InlineData("rls")] [InlineData("prd")]
    public void CreateStartInfo_should_select_compose_and_dotenv_for_environment(string environment)
    {
        var start = ProcessExecutionService.CreateStartInfo("/srv/projects/olivaw-hub/ms-devops-api", "develop", environment, DeploymentStep.Compose);
        Assert.Equal("/srv/projects/olivaw-hub/ms-devops-api", start.WorkingDirectory);
        Assert.Equal(new[] { "compose", "--env-file", $"docker/.env.{environment}", "-f", "docker/docker-compose.yml",
            "-f", $"docker/docker-compose.{environment}.yml", "up", "-d" }, start.ArgumentList.ToArray());
    }

    [Theory]
    [InlineData("dev;id")] [InlineData("../prd")] [InlineData("dev&&id")] [InlineData("dev|id")]
    public void CreateStartInfo_should_reject_environment_injection(string environment)
    {
        Assert.Throws<ValidationException>(() => ProcessExecutionService.CreateStartInfo("/srv/projects/repo", "develop", environment, DeploymentStep.Compose));
    }
}
