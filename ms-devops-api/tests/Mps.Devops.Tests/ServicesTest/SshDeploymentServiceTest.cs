using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Mps.Devops.Api.Services;
using Mps.Devops.Services;
using Xunit;

namespace Mps.Devops.Tests.ServicesTest;

public class SshDeploymentServiceTest
{
    [Fact]
    public async Task SubmitAsync_should_send_json_over_stdin_and_read_job_id()
    {
        var transport = new Mock<IProcessExecutionService>(MockBehavior.Strict);
        var id = Guid.NewGuid(); using var source = new CancellationTokenSource();
        string captured = "";
        transport.Setup(item => item.ExecuteAsync(It.IsAny<string>(), source.Token)).Callback<string, CancellationToken>((payload, token) => captured = payload)
            .ReturnsAsync(new ProcessExecutionResult { Success = true, Output = JsonSerializer.Serialize(new { success = true, deployment = new { id, status = "queued", log = "Queued." } }) });
        var result = await new SshDeploymentService(transport.Object).SubmitAsync(new DeploymentRequest { Folder = "olivaw-hub/ms-devops-api", Branch = "develop", Environment = "dev" }, source.Token);
        Assert.Equal(id, result.Id); Assert.Equal("queued", result.Status);
        using var json = JsonDocument.Parse(captured);
        Assert.Equal("submit", json.RootElement.GetProperty("action").GetString());
        Assert.Equal("olivaw-hub/ms-devops-api", json.RootElement.GetProperty("folder").GetString());
        Assert.Equal("dev", json.RootElement.GetProperty("environment").GetString());
    }

    [Theory]
    [InlineData("not json")] [InlineData("{}")] [InlineData("[]")]
    [InlineData("{\"success\":false,\"error\":\"not_found\"}")]
    [InlineData("{\"success\":false,\"error\":\"invalid_request\"}")]
    public async Task GetAsync_should_reject_invalid_response_or_known_remote_error(string output)
    {
        var transport = new Mock<IProcessExecutionService>();
        transport.Setup(item => item.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ProcessExecutionResult { Success = true, Output = output });
        await Assert.ThrowsAsync<ValidationException>(() => new SshDeploymentService(transport.Object).GetAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_should_map_transport_failure_without_exposing_output()
    {
        var transport = new Mock<IProcessExecutionService>();
        transport.Setup(item => item.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ProcessExecutionResult { Output = "private credentials" });
        var error = await Assert.ThrowsAsync<ValidationException>(() => new SshDeploymentService(transport.Object).GetAsync(Guid.NewGuid(), CancellationToken.None));
        Assert.DoesNotContain("private credentials", error.Message);
    }

    [Fact]
    public async Task SubmitAsync_should_reject_injection_before_transport()
    {
        var transport = new Mock<IProcessExecutionService>(MockBehavior.Strict);
        await Assert.ThrowsAsync<ValidationException>(() => new SshDeploymentService(transport.Object).SubmitAsync(new DeploymentRequest { Folder = "../repo", Branch = "main;id", Environment = "dev" }, CancellationToken.None));
        transport.VerifyNoOtherCalls();
    }
}
