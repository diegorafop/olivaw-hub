using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Devops.Api.Features.ImplantacaoFeature;
using Mps.Devops.Api.Features.ImplantacaoFeature.Commands;
using Mps.Devops.Tests.Mocks;
using Xunit;

namespace Mps.Devops.Tests.ImplantacaoFeatureTest;

public class ImplantacaoControllerTest
{
    [Fact]
    public async Task Post_should_return_200_and_delegate_request_and_token()
    {
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var request = ImplantacaoFakerBuilder.Create();
        var response = new CriarImplantacaoResponse { Log = "Compose: código de saída 0." };
        using var source = new CancellationTokenSource();
        mediator.Setup(item => item.Send(request, source.Token)).ReturnsAsync(response);
        var result = await new ImplantacaoController(mediator.Object).Post(request, source.Token);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(response, ok.Value);
        mediator.Verify(item => item.Send(request, source.Token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }
}
