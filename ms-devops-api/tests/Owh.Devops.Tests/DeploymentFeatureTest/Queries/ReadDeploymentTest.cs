using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Moq;
using Owh.Devops.Api.Features.DeploymentFeature.Queries;
using Owh.Devops.Services;
using Xunit;

namespace Owh.Devops.Tests.DeploymentFeatureTest.Queries;

public class ReadDeploymentTest
{
    [Fact]
    public async Task Handle_should_map_status_and_propagate_id_and_token()
    {
        var service = new Mock<IDeploymentService>(MockBehavior.Strict);
        var id = Guid.NewGuid(); using var source = new CancellationTokenSource();
        service.Setup(item => item.GetAsync(id, source.Token)).ReturnsAsync(new DeploymentStatus { Id = id, Status = "failed", Log = "Pull: exit code 1." });
        var result = await new ReadDeploymentQueryHandler(service.Object, Mock.Of<IDeploymentAccessService>()).Handle(new ReadDeploymentQuery { Id = id }, source.Token);
        Assert.Equal(id, result.Id); Assert.Equal("failed", result.Status); Assert.Equal("Pull: exit code 1.", result.Log);
        service.VerifyAll();
    }

    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new ReadDeploymentQueryHandler(Mock.Of<IDeploymentService>(), Mock.Of<IDeploymentAccessService>()).Handle(null!, CancellationToken.None));
    }

    [Fact]
    public void Validate_should_require_non_empty_id()
    {
        var validator = new ReadDeploymentQueryValidator();
        validator.TestValidate(new ReadDeploymentQuery()).ShouldHaveValidationErrorFor(item => item.Id);
        validator.TestValidate(new ReadDeploymentQuery { Id = Guid.NewGuid() }).ShouldNotHaveAnyValidationErrors();
    }
}
