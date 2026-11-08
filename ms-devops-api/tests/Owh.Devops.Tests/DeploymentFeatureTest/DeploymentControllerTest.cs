using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Owh.Devops.Api.Features.DeploymentFeature;
using Owh.Devops.Api.Features.DeploymentFeature.Commands;
using Owh.Devops.Api.Features.DeploymentFeature.Queries;
using Owh.Devops.Tests.Mocks;
using Xunit;

namespace Owh.Devops.Tests.DeploymentFeatureTest;

public class DeploymentControllerTest
{
    [Fact]
    public async Task Post_should_return_202_with_status_location()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var request = DeploymentFakerBuilder.Create();
        var response = new CreateDeploymentResponse { Id = Guid.NewGuid(), Status = "queued" };
        using var source = new CancellationTokenSource();
        mediator.Setup(item => item.Send(request, source.Token)).ReturnsAsync(response);
        var result = await new DeploymentController(mediator.Object).Create(request, source.Token);
        var accepted = Assert.IsType<AcceptedAtActionResult>(result.Result);
        Assert.Equal(202, accepted.StatusCode); Assert.Equal("Read", accepted.ActionName);
        Assert.Equal(response.Id, accepted.RouteValues!["id"]); Assert.Same(response, accepted.Value);
        mediator.Verify(item => item.Send(request, source.Token), Times.Once);
    }

    [Fact]
    public async Task Get_should_return_200_and_forward_query_and_token()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var request = new ReadDeploymentQuery { Id = Guid.NewGuid() };
        var response = new ReadDeploymentResponse { Id = request.Id, Status = "succeeded", Log = "Compose: exit code 0." };
        using var source = new CancellationTokenSource();
        mediator.Setup(item => item.Send(request, source.Token)).ReturnsAsync(response);
        var result = await new DeploymentController(mediator.Object).Read(request, source.Token);
        Assert.Same(response, Assert.IsType<OkObjectResult>(result.Result).Value);
        mediator.Verify(item => item.Send(request, source.Token), Times.Once);
    }
}
