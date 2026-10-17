using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Moq;
using Mps.Devops.Api.Features.ImplantacaoFeature.Queries;
using Mps.Devops.Services;
using Xunit;

namespace Mps.Devops.Tests.ImplantacaoFeatureTest.Queries;

public class GetDeploymentTest
{
    [Fact]
    public async Task Handle_should_map_status_and_propagate_id_and_token()
    {
        var service = new Mock<IDeploymentService>(MockBehavior.Strict);
        var id = Guid.NewGuid(); using var source = new CancellationTokenSource();
        service.Setup(item => item.GetAsync(id, source.Token)).ReturnsAsync(new DeploymentStatus { Id = id, Status = "failed", Log = "Pull: exit code 1." });
        var result = await new GetDeploymentQueryHandler(service.Object).Handle(new GetDeploymentQuery { Id = id }, source.Token);
        Assert.Equal(id, result.Id); Assert.Equal("failed", result.Status); Assert.Equal("Pull: exit code 1.", result.Log);
        service.VerifyAll();
    }

    [Fact]
    public async Task Handle_should_reject_null_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new GetDeploymentQueryHandler(Mock.Of<IDeploymentService>()).Handle(null!, CancellationToken.None));
    }

    [Fact]
    public void Validate_should_require_non_empty_id()
    {
        var validator = new GetDeploymentQueryValidator();
        validator.TestValidate(new GetDeploymentQuery()).ShouldHaveValidationErrorFor(item => item.Id);
        validator.TestValidate(new GetDeploymentQuery { Id = Guid.NewGuid() }).ShouldNotHaveAnyValidationErrors();
    }
}
