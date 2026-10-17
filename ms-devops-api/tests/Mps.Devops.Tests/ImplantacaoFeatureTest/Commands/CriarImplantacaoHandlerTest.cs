using Mps.Devops.Api.Features.ImplantacaoFeature.Internals;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Mps.Devops.Api.Features.ImplantacaoFeature.Commands;
using Mps.Devops.Services;
using Mps.Devops.Tests.Mocks;
using Xunit;

namespace Mps.Devops.Tests.ImplantacaoFeatureTest.Commands;

public class CriarImplantacaoHandlerTest
{
    [Fact]
    public async Task Handle_should_execute_ordered_steps_and_propagate_token()
    {
        var mock = new Mock<IProcessExecutionService>(MockBehavior.Strict);
        var request = ImplantacaoFakerBuilder.Create();
        using var source = new CancellationTokenSource();
        var sequence = new MockSequence();
        foreach (var step in new[] { DeploymentStep.Checkout, DeploymentStep.Pull, DeploymentStep.Compose })
            mock.InSequence(sequence).Setup(item => item.ExecuteAsync(request.Folder, request.Branch, request.Environment, step, source.Token))
                .ReturnsAsync(new ProcessExecutionResult { Success = true, Log = step.ToString() });
        var response = await new CriarImplantacaoCommandHandler(mock.Object, new DeploymentGate()).Handle(request, source.Token);
        Assert.Equal(string.Join(Environment.NewLine, "Checkout", "Pull", "Compose"), response.Log);
        mock.VerifyAll();
    }

    [Theory]
    [InlineData(DeploymentStep.Checkout)] [InlineData(DeploymentStep.Pull)] [InlineData(DeploymentStep.Compose)]
    public async Task Handle_should_stop_after_operational_failure(DeploymentStep failedStep)
    {
        var calls = new List<DeploymentStep>();
        var mock = new Mock<IProcessExecutionService>();
        mock.Setup(item => item.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DeploymentStep>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, string, DeploymentStep, CancellationToken>((folder, branch, environment, step, token) =>
            {
                calls.Add(step);
                return Task.FromResult(new ProcessExecutionResult { Success = step != failedStep });
            });
        var exception = await Assert.ThrowsAsync<ValidationException>(() => new CriarImplantacaoCommandHandler(mock.Object, new DeploymentGate())
            .Handle(ImplantacaoFakerBuilder.Create(), CancellationToken.None));
        Assert.Single(exception.Errors);
        Assert.Equal((int)failedStep + 1, calls.Count);
        Assert.Equal(failedStep, calls[^1]);
    }

    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        var mock = new Mock<IProcessExecutionService>(MockBehavior.Strict);
        await Assert.ThrowsAsync<ArgumentNullException>(() => new CriarImplantacaoCommandHandler(mock.Object, new DeploymentGate()).Handle(null!, CancellationToken.None));
        mock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_honor_cancellation_before_process()
    {
        var mock = new Mock<IProcessExecutionService>(MockBehavior.Strict);
        using var source = new CancellationTokenSource(); source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new CriarImplantacaoCommandHandler(mock.Object, new DeploymentGate())
            .Handle(ImplantacaoFakerBuilder.Create(), source.Token));
        mock.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task Handle_should_wait_for_gate_and_release_it_after_failure()
    {
        using var gate = new DeploymentGate();
        await gate.Semaphore.WaitAsync();
        var mock = new Mock<IProcessExecutionService>(MockBehavior.Strict);
        mock.Setup(item => item.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), DeploymentStep.Checkout, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessExecutionResult { Success = false });
        var task = new CriarImplantacaoCommandHandler(mock.Object, gate).Handle(ImplantacaoFakerBuilder.Create(), CancellationToken.None);
        Assert.False(task.IsCompleted);
        mock.VerifyNoOtherCalls();
        gate.Semaphore.Release();
        await Assert.ThrowsAsync<ValidationException>(() => task);
        Assert.Equal(1, gate.Semaphore.CurrentCount);
        mock.Verify(item => item.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), DeploymentStep.Checkout, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_propagate_process_cancellation_and_release_gate()
    {
        using var gate = new DeploymentGate();
        using var source = new CancellationTokenSource();
        var mock = new Mock<IProcessExecutionService>();
        mock.Setup(item => item.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), DeploymentStep.Checkout, source.Token))
            .ThrowsAsync(new OperationCanceledException(source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new CriarImplantacaoCommandHandler(mock.Object, gate)
            .Handle(ImplantacaoFakerBuilder.Create(), source.Token));
        Assert.Equal(1, gate.Semaphore.CurrentCount);
    }
}
