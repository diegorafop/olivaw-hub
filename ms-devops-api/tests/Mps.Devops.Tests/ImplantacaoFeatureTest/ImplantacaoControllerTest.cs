using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Devops.Api.Features.ImplantacaoFeature;
using Mps.Devops.Api.Features.ImplantacaoFeature.Commands;
using Mps.Devops.Api.Features.ImplantacaoFeature.Queries;
using Mps.Devops.Tests.Mocks;
using Xunit;

namespace Mps.Devops.Tests.ImplantacaoFeatureTest;

public class ImplantacaoControllerTest
{
    [Fact]
    public async Task Post_should_return_202_with_status_location()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var request = ImplantacaoFakerBuilder.Create();
        var response = new CriarImplantacaoResponse { Id = Guid.NewGuid(), Status = "queued" };
        using var source = new CancellationTokenSource();
        mediator.Setup(item => item.Send(request, source.Token)).ReturnsAsync(response);
        var result = await new ImplantacaoController(mediator.Object).Post(request, source.Token);
        var accepted = Assert.IsType<AcceptedAtActionResult>(result.Result);
        Assert.Equal(202, accepted.StatusCode); Assert.Equal("Get", accepted.ActionName);
        Assert.Equal(response.Id, accepted.RouteValues!["id"]); Assert.Same(response, accepted.Value);
        mediator.Verify(item => item.Send(request, source.Token), Times.Once);
    }

    [Fact]
    public async Task Get_should_return_200_and_forward_query_and_token()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var request = new GetDeploymentQuery { Id = Guid.NewGuid() };
        var response = new GetDeploymentResponse { Id = request.Id, Status = "succeeded", Log = "Compose: exit code 0." };
        using var source = new CancellationTokenSource();
        mediator.Setup(item => item.Send(request, source.Token)).ReturnsAsync(response);
        var result = await new ImplantacaoController(mediator.Object).Get(request, source.Token);
        Assert.Same(response, Assert.IsType<OkObjectResult>(result.Result).Value);
        mediator.Verify(item => item.Send(request, source.Token), Times.Once);
    }
}
