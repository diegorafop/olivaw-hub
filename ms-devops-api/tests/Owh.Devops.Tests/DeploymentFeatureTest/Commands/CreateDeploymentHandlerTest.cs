using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Owh.Devops.Api.Features.DeploymentFeature.Commands;
using Owh.Devops.Services;
using Owh.Devops.Tests.Mocks;
using Xunit;

namespace Owh.Devops.Tests.DeploymentFeatureTest.Commands;

public class CreateDeploymentHandlerTest
{
    [Fact]
    public async Task Handle_should_submit_request_and_propagate_token()
    {
        var service = new Mock<IDeploymentService>(MockBehavior.Strict);
        var request = DeploymentFakerBuilder.Create();
        using var source = new CancellationTokenSource();
        var status = new DeploymentStatus { Id = Guid.NewGuid(), Status = "queued", Log = "Deployment queued." };
        service.Setup(item => item.SubmitAsync(It.Is<DeploymentRequest>(item => item.Folder == request.Folder
            && item.Branch == request.Branch && item.Environment == request.Environment), source.Token)).ReturnsAsync(status);
        var response = await new CreateDeploymentCommandHandler(service.Object, Mock.Of<IDeploymentAccessService>()).Handle(request, source.Token);
        Assert.Equal(status.Id, response.Id); Assert.Equal(status.Status, response.Status); Assert.Equal(status.Log, response.Log);
        service.VerifyAll(); service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        var service = new Mock<IDeploymentService>(MockBehavior.Strict);
        await Assert.ThrowsAsync<ArgumentNullException>(() => new CreateDeploymentCommandHandler(service.Object, Mock.Of<IDeploymentAccessService>()).Handle(null!, CancellationToken.None));
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_should_propagate_known_failure()
    {
        var service = new Mock<IDeploymentService>();
        service.Setup(item => item.SubmitAsync(It.IsAny<DeploymentRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Executor indisponível."));
        await Assert.ThrowsAsync<ValidationException>(() => new CreateDeploymentCommandHandler(service.Object, Mock.Of<IDeploymentAccessService>())
            .Handle(DeploymentFakerBuilder.Create(), CancellationToken.None));
    }
}
