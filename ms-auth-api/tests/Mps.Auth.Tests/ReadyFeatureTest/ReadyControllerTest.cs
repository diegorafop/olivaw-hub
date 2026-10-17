using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Mps.Auth.Api.Features.ReadyFeature;
using Mps.Auth.Api.Features.ReadyFeature.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mps.Auth.Tests.ReadyFeatureTest;

public class ReadyControllerTest
{
    private readonly Mock<IMediator> _mediatorMock = new(MockBehavior.Strict);
    private readonly ReadyController _controller;

    public ReadyControllerTest()
    {
        _controller = new ReadyController(_mediatorMock.Object);
    }

    [Fact]
    public void Constructor_should_have_error_when_mediator_is_null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ReadyController(null!));
        Assert.Equal("mediator", exception.ParamName);
    }

    [Fact]
    public async Task Get_should_send_request_and_return_response()
    {
        var request = new SelecionarReadyQuery();
        var expected = new SelecionarReadyQueryResponse();
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ReturnsAsync(expected);

        var response = await _controller.Get(request);

        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Same(expected, result.Value);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_should_propagate_mediator_error()
    {
        var request = new SelecionarReadyQuery();
        var expected = new InvalidOperationException("fatal error");
        _mediatorMock.Setup(item => item.Send(request, CancellationToken.None)).ThrowsAsync(expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Get(request));

        Assert.Same(expected, exception);
        _mediatorMock.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }
}
