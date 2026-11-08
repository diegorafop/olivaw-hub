using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Owh.Devops.Api.Features.VersionFeature;
using Owh.Devops.Api.Features.VersionFeature.Queries;
using Owh.Devops.Tests.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Owh.Devops.Tests.VersionFeatureTest;

public class VersionControllerTest
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly VersionController _controller;

    private readonly FakerBuilder _fakerBuilder;

    public VersionControllerTest()
    {
        _fakerBuilder = new FakerBuilder();
        _mediatorMock = new Mock<IMediator>();

        _controller = new VersionController
        (
            _mediatorMock.Object
        );
    }

    [Fact]
    public void Constructor_should_have_error()
    {
        IMediator? mediator = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new VersionController(mediator)
        );

        Assert.Equal
        (
            new ArgumentNullException(nameof(mediator)).Message,
            exception.Message
        );
    }

    [Fact]
    public async Task Get_should_not_have_error()
    {
        var result = _fakerBuilder
            .VersionFeature
            .ReadVersionQueryResponse
            .Generate();

        _mediatorMock
            .Setup(item => item.Send(It.IsAny<ReadVersionQuery>(), CancellationToken.None))
            .ReturnsAsync(result);

        var response = await _controller.Get(It.IsAny<ReadVersionQuery>(), CancellationToken.None);

        _mediatorMock.Verify(item => item.Send(It.IsAny<ReadVersionQuery>(), CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();

        Assert.NotNull(response);
        Assert.IsType<OkObjectResult>(response);
        var resultResponse = ((OkObjectResult)response).Value as ReadVersionQueryResponse;
        Assert.NotNull(resultResponse);
        Assert.IsType<ReadVersionQueryResponse>(resultResponse);
        Assert.Equal(result.Version, resultResponse.Version);
        Assert.Equal(result.Title, resultResponse.Title);
    }

    [Fact]
    public async Task Get_should_have_error()
    {
        var result = _fakerBuilder
            .VersionFeature
            .ReadVersionQueryResponse
            .Generate();

        _mediatorMock
            .Setup(item => item.Send(It.IsAny<ReadVersionQuery>(), CancellationToken.None))
            .ReturnsAsync(result)
            .Callback(() =>
            {
                throw new Exception("fatal error");
            });

        Task action() => _controller.Get(It.IsAny<ReadVersionQuery>(), CancellationToken.None);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("fatal error", exception.Message);

        _mediatorMock.Verify(item => item.Send(It.IsAny<ReadVersionQuery>(), CancellationToken.None), Times.Once);
        _mediatorMock.VerifyNoOtherCalls();
    }
}