using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Owh.Olivaw.Api.Features.ReadyFeature;
using Owh.Olivaw.Api.Features.ReadyFeature.Queries;
using Xunit;

namespace Owh.Olivaw.Tests.ReadyFeatureTest;

public sealed class ReadyControllerTest
{
    [Theory]
    [InlineData("pass")]
    [InlineData("fail")]
    public async Task Get_should_forward_request_and_cancellation_and_preserve_http_contract(string status)
    {
        using var cancellation = new CancellationTokenSource();
        var request = new ReadReadyQuery();
        var response = new ReadReadyQueryResponse { Status = status, Details = new List<ReadReadyDetailQueryResponse>() };
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(item => item.Send(request, cancellation.Token)).ReturnsAsync(response);

        var result = Assert.IsType<OkObjectResult>(await new ReadyController(mediator.Object).Get(request, cancellation.Token));

        Assert.Equal(200, result.StatusCode);
        Assert.Same(response, result.Value);
        mediator.Verify(item => item.Send(request, cancellation.Token), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_should_propagate_mediator_error()
    {
        var request = new ReadReadyQuery();
        var expected = new InvalidOperationException("Simulated mediator failure");
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        mediator.Setup(item => item.Send(request, CancellationToken.None)).ThrowsAsync(expected);

        Assert.Same(expected, await Assert.ThrowsAsync<InvalidOperationException>(() => new ReadyController(mediator.Object).Get(request, CancellationToken.None)));
        mediator.Verify(item => item.Send(request, CancellationToken.None), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public void Constructor_should_reject_missing_mediator()
    {
        Assert.Equal("mediator", Assert.Throws<ArgumentNullException>(() => new ReadyController(null!)).ParamName);
    }
}
